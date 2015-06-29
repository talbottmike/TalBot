namespace TalBot

open TalBot
open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open System.Text
open System

type TicketInfo = FSharp.Data.JsonProvider<"..\TalBot\TicketSample.json">

//http://fsharpforfunandprofit.com/posts/computation-expressions-intro/
type OrElseBuilder() =
    member this.ReturnFrom(x) = x
    member this.Combine (a,b) = 
        match a with
        | Some _ -> a  // a succeeds -- use it
        | None -> b    // a fails -- use b instead
    member this.Delay(f) = f()

type Jira(jiraConfiguration)=

    let orElse = new OrElseBuilder()
    let ticketLookupUri = jiraConfiguration.BaseUri + "rest/api/2/issue/"
    let ticketUriPrefix = jiraConfiguration.BaseUri + "browse/"
    let encodedTicketCredentials =
        let byteCredentials = UTF8Encoding.UTF8.GetBytes(jiraConfiguration.TicketCredentials)
        Convert.ToBase64String(byteCredentials)

    let ticketRequest ticket =
        Http.Request(ticketLookupUri + ticket, headers = [ "Authorization", "Basic " + encodedTicketCredentials ])

    let tryGetInfo ticketNumber =
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
        
    member this.MakeTicketResponse ticketNumber = 
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

    member this.makeLinks matches = 
        matches |> Seq.distinct |> Seq.map this.MakeTicketResponse |> Seq.choose (fun x -> x) |> String.concat "\n" 

    member this.TicketRegex = jiraConfiguration.TicketRegex