module TalBot.Configuration

open Microsoft.Azure
open System.Configuration
open TalBot
open FSharp.Data
open System.Text
open System

type private Config = XmlProvider<"../TalBot.Agent/private.config">

let private getConfig (key:string) = 
    let value = ConfigurationManager.AppSettings.Item(key)

    let valueOption = 
        match value with
        | null -> 
            //None
            Config.Load("private.config").Adds
            |> Seq.filter (fun x -> x.Key = key)
            |> Seq.map (fun x -> x.Value)
            |> Seq.tryHead
        | _ -> Some value

    match valueOption with
    | None -> ""
    | Some x -> x

let serviceBusReadConnectionString = getConfig "ServiceBusReadConnectionString"
let serviceBusWriteConnectionString = CloudConfigurationManager.GetSetting("ServiceBusWriteConnectionString")
let slackToken = getConfig "SlackToken"
let debugChannel = getConfig "DebugChannel"
    
let inDebug = DebugOption.NonDebugMode
//    #if COMPILED 
//        match ConfigurationManager.AppSettings.Item("DebugOption") with
//        | "false" -> DebugOption.NonDebugMode
//        | _ -> DebugOption.DebugMode
//    #else                        
//        DebugOption.NonDebugMode
//    #endif

let ticketRegex = getConfig "TicketRegex"
let ticketBaseUri = getConfig "TicketUri"
let ticketLookupUri = ticketBaseUri + "rest/api/2/issue/"
let ticketUriPrefix = ticketBaseUri + "browse/"
let uri = getConfig "SlackUri"
let private webSocketStartUri = getConfig "SlackWebSocketStartUri"
type private slackStart = JsonProvider<"""{"ok": true,"url": "wss:\/\/ms144.slack-msgs.com\/websocket\/DGQ="}""">
let webSocketUri = slackStart.Load(webSocketStartUri).Url

let ticketCredentials = getConfig "TicketCredentials"
let encodedTicketCredentials =
    let byteCredentials = UTF8Encoding.UTF8.GetBytes(ticketCredentials)
    Convert.ToBase64String(byteCredentials)

let ticketRequest ticket =
    Http.Request(ticketLookupUri + ticket, headers = [ "Authorization", "Basic " + encodedTicketCredentials ])