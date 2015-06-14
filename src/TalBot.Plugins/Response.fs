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
                let joeism = 
                    match Math.Floor(incomingMessage.timestamp) % 2m = 0m with
                    | true -> " light it up!"
                    | false -> " ticket on deck!"

                let getMatches str = 
                    regexMatches ticketRegex str

                let makeLinks matches = 
                    matches |> Seq.map (fun x -> "<" + ticketUriPrefix + x + "|" + x + ">") |> String.concat "\n" 

                let matches = getMatches incomingMessage.text
                match matches with
                | x when Seq.isEmpty x -> 
                    match debugChannel with
                    | "" | null -> ()
                    | _ ->
                        let txt = serializeIncomingMessage incomingMessage
                        buildDebugPayload {OutgoingMessage.destination=debugChannel; sender="TalBot"; text=txt; icon=":smile:";} |> postToSlack
                    
                    {OutgoingMessage.destination="#"+message.channelName; sender="TalBot"; text="I can create links for tickets that match the following regex " + ticketRegex; icon=":stuck_out_tongue_winking_eye:";} 
                | x ->   
                    {OutgoingMessage.destination="#"+message.channelName; sender="TalBot"; text="@" + incomingMessage.userName + joeism + "\n" + (makeLinks x); icon=":smile:";} 

            [| ticketResponse message |] :> seq<OutgoingMessage>

