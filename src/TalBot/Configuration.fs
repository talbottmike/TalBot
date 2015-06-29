namespace TalBot

open Microsoft.Azure
open System.Configuration
open TalBot
open FSharp.Data
open System.Text
open System

module Configuration =
    type private Config = XmlProvider<"../TalBot.Agent/private.config">
    type slackStart = JsonProvider<"""{"ok": true,"url": "wss:\/\/ms144.slack-msgs.com\/websocket\/DGQ="}""">

    let private getConfig interactiveConfigurationPath (key:string) = 
        match interactiveConfigurationPath with
        | None -> 
            match ConfigurationManager.AppSettings.Item(key) with
            | null -> ""
            | x -> x
        | Some (x:string) ->
            let valueOption =
                Config.Load(x).Adds
                |> Seq.filter (fun y -> y.Key = key)
                |> Seq.map (fun y -> y.Value)
                |> Seq.tryHead
            
            match valueOption with
            | None -> ""
            | Some x -> x

    let private jiraConfig interactiveConfigurationPath =
        {
            BaseUri=getConfig interactiveConfigurationPath "TicketUri";
            TicketCredentials=getConfig interactiveConfigurationPath "TicketCredentials";
            TicketRegex=getConfig interactiveConfigurationPath "TicketRegex";
        }

    let private slackConfig interactiveConfigurationPath =
        {
            DebugChannel=getConfig interactiveConfigurationPath "DebugChannel";
            ServiceBusReadConnectionString=getConfig interactiveConfigurationPath "ServiceBusReadConnectionString";
            ServiceBusWriteConnectionString=getConfig interactiveConfigurationPath "ServiceBusWriteConnectionString";
            SlackToken=getConfig interactiveConfigurationPath "SlackToken";
            SlackUri=getConfig interactiveConfigurationPath "SlackUri";
            WebSocketUri=
                let webSocketStartUri = getConfig interactiveConfigurationPath "SlackWebSocketStartUri"
                slackStart.Load(webSocketStartUri).Url
        }

    let slackConfiguration () = slackConfig None
    let slackConfigurationInteractive configurationPath = slackConfig <| Some configurationPath
    let botConfiguration () = {Name="TalBot";} 
    let jiraConfiguration () = jiraConfig None
    let jiraConfigurationInteractive configurationPath = jiraConfig <| Some configurationPath
