namespace TalBot

open FSharp.Data
open Newtonsoft.Json
open System.Configuration
open TalBot

module Bot =
    open System.Text.RegularExpressions
    open System
    
    let blankResponse = { Response.text = ""; username = ""; icon_emoji = "" }
    
    let serializeIncomingMessage incomingMessage = 
            JsonConvert.SerializeObject(incomingMessage)

    let postToSlack payload = 
        let content = JsonConvert.SerializeObject(payload)
        let uri = ConfigurationManager.AppSettings.Item("SlackUri")
        match Uri.TryCreate(uri, UriKind.Absolute) with
        | (true, x) -> Http.RequestString(x.AbsoluteUri.ToString(), body=TextRequest content) |> ignore
        | (false, _) -> ()

    let payload message debugOption =
        let x = 
            {
                channel=message.destination
                username=
                    match message.sender with
                    | "" | null -> "TalBot"
                    | _ -> message.sender 
                text=message.text
                icon_emoji=
                    match message.icon with
                    | "" | null -> ":smile:"
                    | _ -> message.icon
            }
            
        match debugOption with
        | DebugOption.DebugMode -> 
            let debugChannel = ConfigurationManager.AppSettings.Item("DebugChannel")
            { x with channel=debugChannel}
        | DebugOption.NonDebugMode -> x

    let speak debugOption =
        let payload message =
            payload message debugOption

        let plugins = PluginLoader.load

        let pluginResult (plugin:IPlugin) =
            plugin.Run() |> Seq.map (fun x -> Some(x))   

        let pluginResults = 
            Seq.collect pluginResult plugins

        let messages = pluginResults |> Seq.choose (fun x -> x) |> Seq.toList
        let messageLog = MessageLog.Read
        let previousMessages = JsonConvert.DeserializeObject<OutgoingMessage list>(messageLog())
                
        // If there are no previous logs, we'll just log the existing messages instead of potentially double posting due to lost state.
        match (box previousMessages = null) with
        | true -> ()
        | false -> 
            let difference () = (Set.ofList messages) - (Set.ofList previousMessages) |> Set.toList
            difference () |> List.map payload  |> List.iter postToSlack

        let serialized = JsonConvert.SerializeObject(messages)
        MessageLog.Save serialized

    let gossip (incomingMessage:IncomingMessage) =
        let regexMatches pattern input =
           Regex.Matches(input,pattern,RegexOptions.IgnoreCase) 
           |> Seq.cast
           |> Seq.map (fun (regMatch:Match) -> regMatch.Value)

        let getMatches str = 
            let prefixes = ConfigurationManager.AppSettings.Item("TicketPrefixes").Split(',')
            let matchString = prefixes |> Seq.map (fun x -> x + @"\d{1,}") |> Seq.reduce (fun x y -> x + "|" + y)
            regexMatches matchString str

        let makeLinks matches = 
            let uriPrefix = ConfigurationManager.AppSettings.Item("TicketUri")
            matches |> Seq.map (fun x -> "<" + uriPrefix + x + "|" + x + ">") |> String.concat "\n" 

        let matches = getMatches incomingMessage.text
        match matches with
        | x when Seq.isEmpty x -> 
            let debugChannel = ConfigurationManager.AppSettings.Item("DebugChannel")
            match debugChannel with
            | "" | null -> ()
            | _ ->
                let txt = serializeIncomingMessage incomingMessage
                payload {OutgoingMessage.destination=debugChannel; sender="TalBot"; text=txt; icon=":smile:";} DebugOption.DebugMode |> postToSlack
            
            { Response.text = "I can create links for tickets with the following prefixes. " + ConfigurationManager.AppSettings.Item("TicketPrefixes"); username = "TalBot"; icon_emoji = ":stuck_out_tongue_winking_eye:" }
        | x ->   
            {Response.text =  "@" + incomingMessage.userName + ": let me get a link to that for you.\n" + (makeLinks x); username = "TalBot"; icon_emoji = ":smile:" }

    let respond incomingMessage =
        let slackToken = ConfigurationManager.AppSettings.Item("SlackToken")

        match incomingMessage with
        // If the token doesn't match our token, we will tell them to leave us alone.
        | x when not (x.token = slackToken)-> { blankResponse with text="Buzz off! I don't know you." }
        // If the user is slackbot, we won't respond to avoid a loop.
        | x when (x.userName = "slackbot") -> blankResponse      
        // Otherwise we'll come up with a response or talk about it behind your back by posting separately.
        | _ -> gossip incomingMessage

