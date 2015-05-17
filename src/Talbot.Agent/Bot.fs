namespace TalBot.Agent

open System
open System.Diagnostics
open System.Threading
open Newtonsoft.Json
open ArgumentParser
open FSharp.Data
open TalBot

type Payload = {channel:string; username:string; text:string; icon_emoji:string}

type Bot(uri:string, debugOption:DebugOption) =
    let mutable isRunning = true

    member this.Stop() =
        isRunning <- false

    member this.IsRunning 
        with get() = isRunning
        and set(value) = isRunning <- value

    member this.Run =
        let postToSlack payload = 
            let content = JsonConvert.SerializeObject(payload)
            Http.RequestString(uri, body = TextRequest content) |> ignore

        let payload message =
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
            | DebugOption.DebugMode -> { x with channel="SlackBot"}
            | DebugOption.NonDebugMode -> x

        while isRunning do
            try
                let plugins = PluginLoader.load

                let pluginResult (plugin:IPlugin) =
                    plugin.Run() |> Seq.map (fun x -> Some(x))   

                let pluginResults = 
                    Seq.collect pluginResult plugins

                let messages = pluginResults |> Seq.choose (fun x -> x) |> Seq.toList
                let messageLog = MessageLog.Read
                let previousMessages = JsonConvert.DeserializeObject<Message list>(messageLog())
                
                // If there are no previous logs, we'll just log the existing messages instead of potentially double posting due to lost state.
                match (box previousMessages = null) with
                | true -> ()
                | false -> 
                    let difference () = (Set.ofList messages) - (Set.ofList previousMessages) |> Set.toList
                    difference () |> List.map payload  |> List.iter postToSlack

                let serialized = JsonConvert.SerializeObject(messages)
                MessageLog.Save serialized
                
                let sleepInterval = new TimeSpan(0,5,0)
                let watch = Stopwatch.StartNew()
                while (watch.Elapsed < sleepInterval && isRunning) do
                    Thread.Sleep(250);

            with
            | exn -> 
                printf "Error: %s" exn.Message
                let interval = new TimeSpan(0,5,0)
                let watch = Stopwatch.StartNew()
                while (watch.Elapsed < interval && isRunning) do
                    Thread.Sleep(250);