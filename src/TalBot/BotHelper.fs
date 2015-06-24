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
open TalBot.Extensions

// Blank response can be used to ignore to an incoming message
let blankResponse = { Response.text = ""; username = ""; icon_emoji = "" }
type MessageType = JsonProvider<"""{"type": "hello","subtype": "bot_message"}""">
type Message = JsonProvider<"""{"reply_to":10000,"type":"message","channel":"C04FOOBAR","user":"U04FOOBAR","text":"test-7","ts":"1434721567.000002"}""">
type BotMessage = JsonProvider<"""{"type": "message","subtype": "bot_message","ts": "1358877455.000010","text": "Pushing is the answer","bot_id": "BFOOBAR","username": "github","icons": {}}""">

type SlackMessageType = 
    | HELLO
    | MESSAGE
    | BOTMESSAGE
//    | USER_TYPING
//    | CHANNEL_MARKED
//    | CHANNEL_CREATED
//    | CHANNEL_JOINED
//    | CHANNEL_LEFT
//    | CHANNEL_DELETED
//    | CHANNEL_RENAME
//    | CHANNEL_ARCHIVE
//    | CHANNEL_UNARCHIVE
//    | CHANNEL_HISTORY_CHANGED
//    | IM_CREATED
//    | IM_OPEN
//    | IM_CLOSE
//    | IM_MARKED
//    | IM_HISTORY_CHANGED
//    | GROUP_JOINED
//    | GROUP_LEFT
//    | GROUP_OPEN
//    | GROUP_CLOSE
//    | GROUP_ARCHIVE
//    | GROUP_UNARCHIVE
//    | GROUP_RENAME
//    | GROUP_MARKED
//    | GROUP_HISTORY_CHANGED
//    | FILE_CREATED
//    | FILE_SHARED
//    | FILE_UNSHARED
//    | FILE_PUBLIC
//    | FILE_PRIVATE
//    | FILE_CHANGE
//    | FILE_DELETED
//    | FILE_COMMENT_ADDED
//    | FILE_COMMENT_EDITED
//    | FILE_COMMENT_DELETED
//    | PIN_ADDED
//    | PIN_REMOVED
//    | PRESENCE_CHANGE
//    | MANUAL_PRESENCE_CHANGE
//    | PREF_CHANGE
//    | USER_CHANGE
//    | TEAM_JOIN
//    | STAR_ADDED
//    | STAR_REMOVED
//    | EMOJI_CHANGED
//    | COMMANDS_CHANGED
//    | TEAM_PLAN_CHANGE
//    | TEAM_PREF_CHANGE
//    | TEAM_RENAME
//    | TEAM_DOMAIN_CHANGE
//    | EMAIL_DOMAIN_CHANGED
//    | BOT_ADDED
//    | BOT_CHANGED
//    | ACCOUNTS_CHANGED
//    | TEAM_MIGRATION_STARTED

let matchMessageType (message:string) =
    let messageType = MessageType.Parse(message)
    match messageType.Type with
    | "hello" -> Some HELLO
    | "message" ->
        match messageType.Subtype with
        | "bot_message" -> Some BOTMESSAGE
        | _ -> Some MESSAGE
//    | "user_typing" -> Some USER_TYPING
//    | "channel_marked" -> Some CHANNEL_MARKED
//    | "channel_created" -> Some CHANNEL_CREATED
//    | "channel_joined" -> Some CHANNEL_JOINED
//    | "channel_left" -> Some CHANNEL_LEFT
//    | "channel_deleted" -> Some CHANNEL_DELETED
//    | "channel_rename" -> Some CHANNEL_RENAME
//    | "channel_archive" -> Some CHANNEL_ARCHIVE
//    | "channel_unarchive" -> Some CHANNEL_UNARCHIVE
//    | "channel_history_changed" -> Some CHANNEL_HISTORY_CHANGED
//    | "im_created" -> Some IM_CREATED
//    | "im_open" -> Some IM_OPEN
//    | "im_close" -> Some IM_CLOSE
//    | "im_marked" -> Some IM_MARKED
//    | "im_history_changed" -> Some IM_HISTORY_CHANGED
//    | "group_joined" -> Some GROUP_JOINED
//    | "group_left" -> Some GROUP_LEFT
//    | "group_open" -> Some GROUP_OPEN
//    | "group_close" -> Some GROUP_CLOSE
//    | "group_archive" -> Some GROUP_ARCHIVE
//    | "group_unarchive" -> Some GROUP_UNARCHIVE
//    | "group_rename" -> Some GROUP_RENAME
//    | "group_marked" -> Some GROUP_MARKED
//    | "group_history_changed" -> Some GROUP_HISTORY_CHANGED
//    | "file_created" -> Some FILE_CREATED
//    | "file_shared" -> Some FILE_SHARED
//    | "file_unshared" -> Some FILE_UNSHARED
//    | "file_public" -> Some FILE_PUBLIC
//    | "file_private" -> Some FILE_PRIVATE
//    | "file_change" -> Some FILE_CHANGE
//    | "file_deleted" -> Some FILE_DELETED
//    | "file_comment_added" -> Some FILE_COMMENT_ADDED
//    | "file_comment_edited" -> Some FILE_COMMENT_EDITED
//    | "file_comment_deleted" -> Some FILE_COMMENT_DELETED
//    | "pin_added" -> Some PIN_ADDED
//    | "pin_removed" -> Some PIN_REMOVED
//    | "presence_change" -> Some PRESENCE_CHANGE
//    | "manual_presence_change" -> Some MANUAL_PRESENCE_CHANGE
//    | "pref_change" -> Some PREF_CHANGE
//    | "user_change" -> Some USER_CHANGE
//    | "team_join" -> Some TEAM_JOIN
//    | "star_added" -> Some STAR_ADDED
//    | "star_removed" -> Some STAR_REMOVED
//    | "emoji_changed" -> Some EMOJI_CHANGED
//    | "commands_changed" -> Some COMMANDS_CHANGED
//    | "team_plan_change" -> Some TEAM_PLAN_CHANGE
//    | "team_pref_change" -> Some TEAM_PREF_CHANGE
//    | "team_rename" -> Some TEAM_RENAME
//    | "team_domain_change" -> Some TEAM_DOMAIN_CHANGE
//    | "email_domain_changed" -> Some EMAIL_DOMAIN_CHANGED
//    | "bot_added" -> Some BOT_ADDED
//    | "bot_changed" -> Some BOT_CHANGED
//    | "accounts_changed" -> Some ACCOUNTS_CHANGED
//    | "team_migration_started" -> Some TEAM_MIGRATION_STARTED
    | _ -> None

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
            | DebugOption.DebugMode -> 
                printfn "IN DEBUG MODE"
                Configuration.debugChannel
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

let attemptToLog (exn:exn) =
    try
        let message = {OutgoingMessage.destination=debugChannel; sender="TalBot"; text=exn.ToDetailedString; icon=":open_mouth:";}
        buildDebugPayload message |> postToSlack
    with
    | exn -> 
        printfn "Failed to log message: %s" exn.ToDetailedString

let getMatches str = 
    regexMatches "\w{2,10}-\d{1,5}" str

let makeLinks matches = 
    matches |> Seq.map Jira.makeTicketResponse |> Seq.choose (fun x -> x) |> String.concat "\n" 

#nowarn "40"
let listenerAgent = MailboxProcessor.Start(fun inbox-> 
    // the message processing function
    let rec messageLoop = async{
        
        // read a message
        let! msg = inbox.Receive()

        try
            let mt = MessageType.Parse(msg)
            match matchMessageType msg with
            | None -> ()
            | Some SlackMessageType.HELLO -> 
                printfn "slack says %s" mt.Type
                let payload = 
                    buildPayload {OutgoingMessage.destination="#bot-log"; sender="TalBot"; text="The old bot was a wooden toy. I'm a real boy."; icon=":talbot:"}
                postToSlack payload
            | Some SlackMessageType.MESSAGE ->
                printfn "message is: %s" msg
                let m = Message.Parse(msg)
                match getMatches m.Text with
                    | x when Seq.isEmpty x -> ()
                    | x ->
                        x |> Seq.map (fun y -> Jira.makeTicketResponse y)
                        |> Seq.choose (fun y -> y)
                        |> Seq.map (fun y -> buildPayload {OutgoingMessage.destination=m.Channel; sender="TalBot"; text= y; icon=":talbot:"})
                        |> Seq.iter postToSlack
            | Some SlackMessageType.BOTMESSAGE ->
                printfn "bot_message is: %s" msg
        with
        | exn -> 
            attemptToLog exn

        // loop to top
        return! messageLoop  
        }

    // start the loop 
    messageLoop 
    )