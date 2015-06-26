module Jira

open TalBot
open FSharp.Data
open TalBot.Configuration
open FSharp.Data.HttpRequestHeaders

type private TicketInfo = FSharp.Data.JsonProvider<"..\TalBot\TicketSample.json">

//http://fsharpforfunandprofit.com/posts/computation-expressions-intro/
type OrElseBuilder() =
    member this.ReturnFrom(x) = x
    member this.Combine (a,b) = 
        match a with
        | Some _ -> a  // a succeeds -- use it
        | None -> b    // a fails -- use b instead
    member this.Delay(f) = f()

let orElse = new OrElseBuilder()

let private tryGetInfo ticketNumber =
    try
        printfn "trying %s" ticketNumber
        let response = 
            Http.RequestString(
                ticketLookupUri + ticketNumber,
                headers = [ "Authorization", "Basic " + encodedTicketCredentials; Accept HttpContentTypes.Json ])
        let result = TicketInfo.Parse(response)
        Some (result)
    with
    | :? System.Net.WebException -> 
        None

let info ticketNumber = orElse {
    return! tryGetInfo ticketNumber
    return! tryGetInfo ("HLG" + ticketNumber)
    return! tryGetInfo ("CSWI" + ticketNumber)
    return! tryGetInfo ("CSWISF" + ticketNumber)
    }

let fields ticketNumber = 
    match info ticketNumber  with
    | Some x -> Some x.Fields
    | None -> None

let summary ticketNumber = 
    match fields ticketNumber with
    | Some x -> Some x.Summary
    | None -> None

let makeTicketResponse ticketNumber = 
    match info ticketNumber with
    | Some x -> 
        let summary = 
            let value = x.Fields.Summary 
            match value with
            | null -> ""
            | _ when value.Length < 130 -> value
            | _ -> value.Substring(0,130) + "..."
        Some ("<" + ticketUriPrefix + x.Key + "|" + x.Key + "> " + summary)
    | None -> None

let makeLinks matches = 
    matches |> Seq.distinct |> Seq.map makeTicketResponse |> Seq.choose (fun x -> x) |> String.concat "\n" 

let ticketRegex = "\w{2,10}-\d{1,5}"