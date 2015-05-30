module Responses

open BotHelper
open TalBot.Configuration
open System.Text.RegularExpressions
open TalBot

// Blank response can be used to ignore to an incoming message
let blankResponse = { Response.text = ""; username = ""; icon_emoji = "" }

// Get all matches for a regex pattern
let regexMatches pattern input =
    Regex.Matches(input,pattern,RegexOptions.IgnoreCase) 
    |> Seq.cast
    |> Seq.map (fun (regMatch:Match) -> regMatch.Value)

// Create a response with a link to the ticket referenced in the incoming message
let ticketResponse (incomingMessage:IncomingMessage) =
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
            
        { Response.text = "I can create links for tickets that match the following regex " + ticketRegex; username = "TalBot"; icon_emoji = ":stuck_out_tongue_winking_eye:" }
    | x ->   
        {Response.text =  "@" + incomingMessage.userName + ": let me get a link to that for you.\n" + (makeLinks x); username = "TalBot"; icon_emoji = ":smile:" }