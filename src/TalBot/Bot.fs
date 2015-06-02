namespace TalBot

open TalBot

module Bot =
    open Configuration
    open Responses
    open BotHelper
    
    // Generates messages by loading and running provided plugins
    let speak =
        let messages = getMessagesFromPlugins
        postNewMessages messages
        saveMessagesToLog messages
    
    // Responds directly to an incoming message and/or posts to queue for workers to process
    let gossip (incomingMessage:IncomingMessage) =
        match useServiceBus with
        | true -> 
            postToServiceQueue incomingMessage
            ticketResponse incomingMessage
            //blankResponse
        | false -> ticketResponse incomingMessage

//        ticketResponse incomingMessage

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
        try
            let message = {OutgoingMessage.destination=debugChannel; sender="TalBot"; text=exn.Message; icon=":open_mouth:";}
            buildDebugPayload message |> postToSlack
        with
        | exn -> 
            printf "Failed to log message: %s" exn.Message

    let slander () =
        printfn "read from service queue"
        readFromServiceQueue ()
