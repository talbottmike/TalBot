module BotHelper

open TalBot.Configuration
open FSharp.Data
open Microsoft.ServiceBus.Messaging
open Newtonsoft.Json
open System
open TalBot

// Serialize incoming message to Json
let serializeIncomingMessage incomingMessage = JsonConvert.SerializeObject(incomingMessage)

// Post incoming message to service bus queue
let postToServiceQueue (incomingMessage:IncomingMessage) =
    let client = QueueClient.CreateFromConnectionString(serviceBusConnectionString, "queue")
    let message = new BrokeredMessage(incomingMessage);
    client.Send(message)

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