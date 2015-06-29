module Agents

open TalBot

/// Agent for posting links to Jira messages. 
/// Uses internal state to reduce chattiness by not reposting a link that has been recently posted.
let jiraAgent (jira:Jira) slackUri:MailboxProcessor<Message.Root> =
    MailboxProcessor.Start(fun inbox ->

        // Loops, keeping a list of messages
        let rec loop (map:Map<string,_>) = async {
            let! msg = inbox.Receive()
            let jiraMatches = Regex.getMatches jira.TicketRegex msg.Text |> Seq.toList
            let slack = Slack.create slackUri

            let previousJiras = 
                match map.TryFind msg.Channel with
                | Some x ->
                    x |> List.collect (fun x -> snd x)
                | None -> List.empty

            jiraMatches 
            |> List.filter (fun x -> not <| List.contains x previousJiras)
            |> Seq.distinct
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

let statefulAgent:MailboxProcessor<Message.Root> =
    MailboxProcessor.Start(fun inbox ->

        // Loops, keeping a list of messages
        let rec loop messages = async {
            let! msg = inbox.Receive()
            printfn "statefulAgent"
//                                match msg with
//                                | x when List.contains x messages -> 
//                                    printfn "%s already exists" x
//                                | x -> 
//                                    printfn "%s" x 

            let newMessages = 
                match messages with
                | [] -> msg::[]
                | x when x.Length > 3 -> msg::(x |> List.take 3)
                | x -> msg::x

            //printfn "%d" newMessages.Length

            return! loop newMessages }    

        loop [] )

let nonStatefulAgent slack (jira:Jira)=
    MailboxProcessor.Start(fun inbox ->
        // Loops, keeping a list of messages
        let rec loop = async {
            let! (msg:Message.Root) = inbox.Receive()
            printfn "nonStatefulAgent"

            let getJiraMatches = Regex.getMatches jira.TicketRegex
            match getJiraMatches msg.Text with
                | x when Seq.isEmpty x -> ()
                | x ->
                    x |> Seq.map (fun y -> jira.MakeTicketResponse y)
                    |> Seq.choose (fun y -> y)
                    |> Seq.map (fun y -> BotHelper.buildPayload {OutgoingMessage.destination=msg.Channel; sender="TalBot"; text= y; icon=":talbot:"})
                    |> Seq.iter (fun x -> Slack.post x slack)

            return! loop }    

        loop )