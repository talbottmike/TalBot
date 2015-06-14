module BotHelper

open TalBot.Configuration
open FSharp.CloudAgent
open FSharp.CloudAgent.Connections
open FSharp.CloudAgent.Messaging
open FSharp.Data
open Microsoft.ServiceBus.Messaging
open Newtonsoft.Json
open System
open System.Text.RegularExpressions
open TalBot

// Blank response can be used to ignore to an incoming message
let blankResponse = { Response.text = ""; username = ""; icon_emoji = "" }

// Get all matches for a regex pattern
let regexMatches pattern input =
    Regex.Matches(input,pattern,RegexOptions.IgnoreCase) 
    |> Seq.cast
    |> Seq.map (fun (regMatch:Match) -> regMatch.Value)

// Serialize incoming message to Json
let serializeIncomingMessage incomingMessage = JsonConvert.SerializeObject(incomingMessage)

// Post payload to slack
let postToSlack payload = 
    let content = JsonConvert.SerializeObject(payload)
        
    match Uri.TryCreate(uri, UriKind.Absolute) with
    | (true, x) -> Http.RequestString(x.AbsoluteUri.ToString(), body=TextRequest content) |> ignore
    | (false, _) -> ()

// Create payload from message
let buildPayload message =
    {
        channel=
            match inDebug with
            | DebugOption.DebugMode -> Configuration.debugChannel
            | DebugOption.NonDebugMode -> message.destination
        username=
            match message.sender with
            | "" | null -> "TalBot"
            | _ -> message.sender 
        text=message.text
        icon_emoji=
            match message.icon with
            | "" | null -> ":smile:"
            | _ -> message.icon
    }

// Create payload from message to post to debug channel      
let buildDebugPayload message =
    let b = buildPayload message
    { b with channel=debugChannel}

// Run notification plugins to get messages
let getMessagesFromNotificationPlugins () =
    let plugins = PluginLoader.loadNotificationPlugins ()

    let pluginResult (plugin:INotificationPlugin) =
        try
            let result = plugin.Run()
            result |> Seq.map (fun x -> (Some(x)))   
        with
        | exn ->             
            // if we fail, we'll just log a brief message and skip this plugin
            printfn "Error running plugin with exception %s" exn.Message
            Seq.empty

    let pluginResults = 
        Seq.collect pluginResult plugins

    pluginResults |> Seq.choose (fun x -> x) |> Seq.toList

// Post only new messages
let postNewMessages (sender,messages) = 
    let messageLog = MessageLog.Read sender
    let previousMessages = JsonConvert.DeserializeObject<OutgoingMessage list>(messageLog)
                
    // If there are no previous logs, we'll just log the existing messages instead of potentially double posting due to lost state.
    match (box previousMessages = null) with
    | true -> ()
    | false -> 
        let difference () = (Set.ofList messages) - (Set.ofList previousMessages) |> Set.toList
        difference () |> List.map buildPayload  |> List.iter postToSlack

// Save messages that were posted to the log
let saveMessagesToLog (sender,messages) =
    let serialized = JsonConvert.SerializeObject(messages)
    MessageLog.Save serialized sender

// Create an Agent on demand to load response plugins and process incoming messages.
let createAgent agentId =
    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! message = inbox.Receive()
                let responsePlugins = PluginLoader.loadResponsePlugins ()

                let responsePluginResult (plugin:IResponsePlugin) = 
                    plugin.Listen message |> Seq.map (fun x -> Some(x))

                let responsePluginResults =
                    Seq.collect responsePluginResult responsePlugins

                responsePluginResults |> Seq.choose (fun x -> x) |> Seq.map buildPayload |> Seq.iter postToSlack         
        })

// Post incoming message to service bus queue
let postToServiceQueue (incomingMessage:IncomingMessage) =
    let serviceBusWriteConnection = ServiceBusConnection serviceBusWriteConnectionString
    let cloudWriteConnection = WorkerCloudConnection(serviceBusWriteConnection, Queue "queue")
    let sendToMessageQueue = ConnectionFactory.SendToWorkerPool cloudWriteConnection
    Async.RunSynchronously (sendToMessageQueue incomingMessage)

// Listen for incoming message from the service bus queue and process using the provided agent
let readFromServiceQueue () =
    let serviceBusReadConnection = ServiceBusConnection serviceBusReadConnectionString
    let cloudReadConnection = WorkerCloudConnection(serviceBusReadConnection, Queue "queue")
    ConnectionFactory.StartListening(cloudReadConnection, createAgent >> Messaging.CloudAgentKind.BasicCloudAgent)