namespace TalBot

open TalBot
open Configuration
open BotHelper

type Bot(botConfiguration,slackConfiguration) = 

    /// Generates messages by loading and running provided plugins
    member this.Speak () =
        let notifications = Notifier.getNotifications ()
        let groupedNotifications = notifications |> List.groupBy (fun x -> x.sender)

        let postAndLog (sender,notifications) = 
            let postNewMessagesToSlack = postNewMessages slackConfiguration.SlackUri
            postNewMessagesToSlack (sender,notifications)
            saveMessagesToLog (sender,notifications)

        groupedNotifications |> List.iter postAndLog
                
    /// Posts to queue for workers to process
    member this.Gossip (incomingMessage:IncomingMessage) =
            Gossiper.spread slackConfiguration.ServiceBusWriteConnectionString incomingMessage
            blankResponse

    /// Evaluates a suspicious incoming message and responds or passes it along with approval to process
    member this.Respond suspectIncomingMessage =
        match suspectIncomingMessage with
        | None -> blankResponse
        // If the token doesn't match our token, we will tell them to leave us alone.
        | Some x when not (x.token = slackConfiguration.SlackToken)-> { blankResponse with text="Buzz off! I don't know you." }
        // If the user is slackbot, we won't respond to avoid a loop.
        | Some x when (x.userName = "slackbot") -> blankResponse      
        // Otherwise we'll come up with a response or talk about it behind your back by posting separately.
        | Some incomingMessage -> this.Gossip incomingMessage

    /// Safely logs an exception to the debug channel
    /// Swallows any exceptions that may occur if the log attempt fails
    member this.AttemptToLog (exn:exn) =
        BotHelper.attemptToLog slackConfiguration.SlackUri slackConfiguration.DebugChannel exn

    member this.Say channel text =
        let payload = buildPayload {OutgoingMessage.destination=channel; sender="TalBot"; text=text; icon=":talbot:"}
        let slack = Slack.create slackConfiguration.SlackUri
        Slack.post payload slack

    member this.Listen () =
        let jira = Jira(Configuration.jiraConfiguration ())        
        let messageHandlers = [Agents.jiraAgent jira slackConfiguration.SlackUri;]
        let listener = Listener(slackConfiguration.SlackUri, slackConfiguration.DebugChannel, messageHandlers)
        listener.Listen slackConfiguration.WebSocketUri

    member this.Slander () =
        let slanderer = Slanderer(slackConfiguration.SlackUri)
        slanderer.Slander slackConfiguration.ServiceBusReadConnectionString
