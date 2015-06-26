module Listener

open BotHelper
open TalBot
open System.Net.WebSockets
open System.Threading
open System

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
                let getJiraMatches = getMatches Jira.ticketRegex
                match getJiraMatches m.Text with
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

let listen webSocketUri =
    let webSocket = new ClientWebSocket()
    Async.AwaitTask (webSocket.ConnectAsync(new Uri(webSocketUri),CancellationToken.None)) |> ignore
    Thread.Sleep (TimeSpan.FromMilliseconds(1000.00))

    let receive (webSocket : ClientWebSocket) =
        let buffer = Array.zeroCreate 6000
        while webSocket.State = WebSocketState.Open do
            let x = Async.AwaitTask (webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None))
            let y = Async.RunSynchronously x 
            match y.MessageType with
            | WebSocketMessageType.Close ->
                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).RunSynchronously |> ignore
                webSocket.Dispose |> ignore
                ()
            | WebSocketMessageType.Text ->
                let s = (System.Text.Encoding.Default.GetString(buffer))
                listenerAgent.Post (s.Substring(0, y.Count))
            | _ -> ()
    receive webSocket