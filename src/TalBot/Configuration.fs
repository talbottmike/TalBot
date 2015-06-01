module TalBot.Configuration

open Microsoft.Azure
open System.Configuration
open TalBot

let debugChannel = ConfigurationManager.AppSettings.Item("DebugChannel")

let inDebug = 
    match ConfigurationManager.AppSettings.Item("DebugOption") with
    | "true" -> DebugOption.DebugMode
    | _ -> DebugOption.NonDebugMode

let serviceBusReadConnectionString = ConfigurationManager.AppSettings.Item("ServiceBusReadConnectionString")
let serviceBusWriteConnectionString = CloudConfigurationManager.GetSetting("ServiceBusWriteConnectionString")
let slackToken = ConfigurationManager.AppSettings.Item("SlackToken")
let ticketRegex = ConfigurationManager.AppSettings.Item("TicketRegex")
let ticketUriPrefix = ConfigurationManager.AppSettings.Item("TicketUri")
let uri = ConfigurationManager.AppSettings.Item("SlackUri")

let useServiceBus = 
    match ConfigurationManager.AppSettings.Item("UseServiceBus") with
    | "true" -> true
    | _ -> false