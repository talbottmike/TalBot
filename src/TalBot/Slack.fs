namespace TalBot

module Slack =
    open Newtonsoft.Json
    open System
    open FSharp.Data

    // Slack.T is the primary type for this module
    type T = {Uri:string;}
    
    // constructor
    let create uri = 
        {T.Uri=uri;}

    // Post payload to slack
    let post payload {T.Uri=uri;} =
        let content = JsonConvert.SerializeObject(payload)
        
        match Uri.TryCreate(uri, UriKind.Absolute) with
        | (true, x) -> Http.RequestString(x.AbsoluteUri.ToString(), body=TextRequest content) |> ignore
        | (false, _) -> ()