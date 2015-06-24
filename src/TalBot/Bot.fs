namespace TalBot

open TalBot

module Bot =
    open Configuration
    open BotHelper
    open System.Net.WebSockets
    open System.Threading
    open System.Threading.Tasks
    open System
    
    // Generates messages by loading and running provided plugins
    let speak () =
        let messages = getMessagesFromNotificationPlugins ()
        printfn "Message count: %d" messages.Length
        let groupedMessages = messages |> List.groupBy (fun x -> x.sender)

        let postAndLog (sender,messages) = 
            postNewMessages (sender,messages)
            saveMessagesToLog (sender,messages)

        groupedMessages |> List.iter postAndLog
                
    // Posts to queue for workers to process
    let gossip (incomingMessage:IncomingMessage) =
            postToServiceQueue incomingMessage
            blankResponse

    // Evaluates a suspicious incoming message and responds or passes it along with approval to process
    let respond suspectIncomingMessage =
        match suspectIncomingMessage with
        | None -> blankResponse
        // If the token doesn't match our token, we will tell them to leave us alone.
        | Some x when not (x.token = slackToken)-> { blankResponse with text="Buzz off! I don't know you." }
        // If the user is slackbot, we won't respond to avoid a loop.
        | Some x when (x.userName = "slackbot") -> blankResponse      
        // Otherwise we'll come up with a response or talk about it behind your back by posting separately.
        | Some incomingMessage -> gossip incomingMessage

    // Safely logs an exception to the debug channel
    // Swallows any exceptions that may occur if the log attempt fails
    let attemptToLog (exn:exn) =
        BotHelper.attemptToLog exn

    let say channel text =
        let payload = buildPayload {OutgoingMessage.destination=channel; sender="TalBot"; text=text; icon=":talbot:"}
        postToSlack payload

    let listen () =
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

    let slander () =
        readFromServiceQueue ()
