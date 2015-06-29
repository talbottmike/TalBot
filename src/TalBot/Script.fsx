#r @"C:\Workspaces\Personal\TalBot\packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#r @"C:\Workspaces\Personal\TalBot\src\TalBot\bin\Debug\TalBot.dll"
open TalBot
open FSharp.Data
open System
open System.Text.RegularExpressions
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks
open System.Configuration

let configPath = "C:\Workspaces\Personal\TalBot\src\Talbot.Agent\private.config"
let jiraConfig = Configuration.jiraConfigurationInteractive configPath
let botConfig = Configuration.botConfiguration ()
let slackConfig = Configuration.slackConfigurationInteractive configPath

let jira = Jira(jiraConfig)
let slack = Slack.create slackConfig.SlackUri
let testAgent:MailboxProcessor<Message.Root> =
    MailboxProcessor.Start(fun inbox ->

        // Loops, keeping a list of messages
        let rec loop (map:Map<string,_>) = async {
            let! msg = inbox.Receive()
            let jiraMatches = Regex.getMatches jira.TicketRegex msg.Text |> Seq.toList

            let previousJiras = 
                match map.TryFind msg.Channel with
                | Some x ->
                    x |> List.collect (fun x -> snd x)
                | None -> List.empty

            jiraMatches 
            |> List.filter (fun x -> not <| List.contains x previousJiras)
            |> Seq.map (fun y -> jira.MakeTicketResponse y)
            |> Seq.choose (fun y -> y)
            |> Seq.map (fun y -> BotHelper.buildPayload {OutgoingMessage.destination=msg.Channel; sender="TalBot"; text= y; icon=":talbot:"})
            |> Seq.iter (fun x -> Slack.post x slack)
            
            let messagesOption = map.TryFind(msg.Channel)

            let newMessages = 
                match messagesOption with
                | None -> (msg,jiraMatches)::[]
                | Some messages ->
                    match messages with 
                    | [] -> (msg,jiraMatches)::[]
                    | x when x.Length > 3 -> (msg,jiraMatches)::(x |> List.take 3)
                    | x -> (msg,jiraMatches)::x

            let newMap = map |> Map.remove msg.Channel |> Map.add msg.Channel newMessages

            return! loop newMap }    

        loop Map.empty )

let messageHandlers = [testAgent;]
let listener = Listener(slackConfig.SlackUri, slackConfig.DebugChannel,messageHandlers)
listener.Listen slackConfig.WebSocketUri

