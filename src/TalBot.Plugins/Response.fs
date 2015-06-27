namespace TalBot.Plugins.Response

open System
open TalBot
open System.Collections.Generic
open TalBot.Configuration
open BotHelper

type TicketResponsePlugin() =
    interface IResponsePlugin with
        member x.Listen message: IEnumerable<OutgoingMessage> =    
            // Create a response with a link to the ticket referenced in the incoming message
            let ticketResponse (incomingMessage:IncomingMessage) =   
                let ticketRegex = "\w{2,10}-\d{1,5}" 
                let ticketUriPrefix = "http://jira.com/browse/"
                let getMatches str = 
                    Regex.getMatches ticketRegex str

                let makeLinks matches = 
                    matches |> Seq.map (fun x -> "<" + ticketUriPrefix + x + "|" + x + ">") |> String.concat "\n" 

                let matches = getMatches incomingMessage.text
                match matches with
                | x when Seq.isEmpty x -> 
                    {OutgoingMessage.destination="#"+message.channelName; sender="TalBot"; text="I can create links for tickets that match the following regex " + ticketRegex; icon=":stuck_out_tongue_winking_eye:";} 
                | x ->   
                    {OutgoingMessage.destination="#"+message.channelName; sender="TalBot"; text="@" + incomingMessage.userName + "\n" + (makeLinks x); icon=":smile:";} 

            [| ticketResponse message |] :> seq<OutgoingMessage>

