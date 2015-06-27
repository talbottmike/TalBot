namespace TalBot

type SlackConfiguration =
    {
        DebugChannel:string;        
        ServiceBusReadConnectionString:string;
        ServiceBusWriteConnectionString:string;
        SlackToken:string;
        SlackUri:string;
        WebSocketUri:string;
    }

type BotConfiguration = 
    {
        Name:string;
    }

type JiraConfiguration =
    {
        BaseUri:string;
        TicketCredentials:string;
        TicketRegex:string;
    }

open Microsoft.Azure
open System.Configuration
open TalBot
open FSharp.Data
open System.Text
open System

module Configuration =
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

    let private debugChannel = getConfig "DebugChannel"
    let private serviceBusReadConnectionString = getConfig "ServiceBusReadConnectionString"
    let private serviceBusWriteConnectionString = CloudConfigurationManager.GetSetting("ServiceBusWriteConnectionString")
    let private slackToken = getConfig "SlackToken"
    let private ticketRegex = getConfig "TicketRegex"
    let private ticketBaseUri = getConfig "TicketUri"
    let private ticketCredentials = getConfig "TicketCredentials"
    let private uri = getConfig "SlackUri"
    let private webSocketStartUri = getConfig "SlackWebSocketStartUri"
    type private slackStart = JsonProvider<"""{"ok": true,"url": "wss:\/\/ms144.slack-msgs.com\/websocket\/DGQ="}""">
    let private webSocketUri = slackStart.Load(webSocketStartUri).Url

    let slackConfiguration () = 
        {
            DebugChannel=debugChannel;
            ServiceBusReadConnectionString=serviceBusReadConnectionString;
            ServiceBusWriteConnectionString=serviceBusWriteConnectionString;
            SlackToken=slackToken;
            SlackUri=uri;
            WebSocketUri=webSocketUri;
        }

    let botConfiguration () = 
        {
            Name="TalBot";
        }

    let jiraConfiguration () =
        {
            BaseUri=ticketBaseUri;
            TicketCredentials=ticketCredentials;
            TicketRegex=ticketRegex;
        }
