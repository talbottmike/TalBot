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

// Create a response with a link to the ticket referenced in the incoming message
let ticketResponse (incomingMessage:IncomingMessage) =    
    let joeism = 
        match Math.Floor(incomingMessage.timestamp) % 2m = 0m with
        | true -> " light it up!"
        | false -> " ticket on deck!"

    let getMatches str = 
        regexMatches ticketRegex str

    let makeLinks matches = 
        matches |> Seq.map (fun x -> "<" + ticketUriPrefix + x + "|" + x + ">") |> String.concat "\n" 

    let matches = getMatches incomingMessage.text
    match matches with
    | x when Seq.isEmpty x -> 
        match debugChannel with
        | "" | null -> ()
        | _ ->
            let txt = serializeIncomingMessage incomingMessage
            buildDebugPayload {OutgoingMessage.destination=debugChannel; sender="TalBot"; text=txt; icon=":smile:";} |> postToSlack
            
        { Response.text = "I can create links for tickets that match the following regex " + ticketRegex; username = "TalBot"; icon_emoji = ":stuck_out_tongue_winking_eye:" }
    | x ->   
        {Response.text =  "@" + incomingMessage.userName + joeism + "\n" + (makeLinks x); username = "TalBot"; icon_emoji = ":smile:" }

// Create a response with a link to the ticket referenced in the incoming message
let postTicketResponse (incomingMessage:IncomingMessage) =    
    let response = ticketResponse incomingMessage
    let payload = buildPayload {OutgoingMessage.destination="#"+incomingMessage.channelName; sender=response.username; text=response.text; icon=response.icon_emoji;}
    postToSlack payload

// Run notification plugins to get messages
let getMessagesFromPlugins =
    let plugins = PluginLoader.load

    let pluginResult (plugin:INotificationPlugin) =
        plugin.Run() |> Seq.map (fun x -> Some(x))   

    let pluginResults = 
        Seq.collect pluginResult plugins

    pluginResults |> Seq.choose (fun x -> x) |> Seq.toList

// Post only new messages
let postNewMessages messages = 
    let messageLog = MessageLog.Read
    let previousMessages = JsonConvert.DeserializeObject<OutgoingMessage list>(messageLog())
                
    // If there are no previous logs, we'll just log the existing messages instead of potentially double posting due to lost state.
    match (box previousMessages = null) with
    | true -> ()
    | false -> 
        let difference () = (Set.ofList messages) - (Set.ofList previousMessages) |> Set.toList
        difference () |> List.map buildPayload  |> List.iter postToSlack

// Save messages that were posted to the log
let saveMessagesToLog messages =
    let serialized = JsonConvert.SerializeObject(messages)
    MessageLog.Save serialized

// A function which creates an Agent on demand.
let createAgent agentId =
    MailboxProcessor.Start(fun inbox ->
        async {
            while true do
                let! message = inbox.Receive()
                printfn "%s is the channelName." message.channelName
                postTicketResponse message
                printfn "text: %s" message.text                
        })

// Post incoming message to service bus queue
let postToServiceQueue (incomingMessage:IncomingMessage) =
    let serviceBusWriteConnection = ServiceBusConnection serviceBusWriteConnectionString
    let cloudWriteConnection = WorkerCloudConnection(serviceBusWriteConnection, Queue "queue")
    let sendToMessageQueue = ConnectionFactory.SendToWorkerPool cloudWriteConnection
    Async.RunSynchronously (sendToMessageQueue incomingMessage)

let readFromServiceQueue () =
    let serviceBusReadConnection = ServiceBusConnection serviceBusReadConnectionString
    let cloudReadConnection = WorkerCloudConnection(serviceBusReadConnection, Queue "queue")
    ConnectionFactory.StartListening(cloudReadConnection, createAgent >> Messaging.CloudAgentKind.BasicCloudAgent)