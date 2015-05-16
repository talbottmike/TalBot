﻿namespace TalBot.Agent

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
              
        let folderMonitorPayload message = 
            match debugOption with
            | DebugOption.DebugMode -> {channel="@mtalbott"; username="StatusBot"; text=message; icon_emoji=":c3po:"}
            // TODO revisit this. Making the same for both cases for now.
            | DebugOption.NonDebugMode -> {channel="@mtalbott"; username="StatusBot"; text=message; icon_emoji=":c3po:"}

        // TODO fix incomplete pattern match
        let payload statusMessage =
            match statusMessage.source with
            | "FolderMonitor" -> folderMonitorPayload statusMessage.message

        while isRunning do
            try
                let plugins = Plugins.load

                let pluginResult (plugin:IPlugin) =
                    plugin.Run() |> Seq.map (fun x -> Some(x))   

                let pluginResults = 
                    Seq.collect pluginResult plugins

                let messages = pluginResults |> Seq.choose (fun x -> x) |> Seq.toList
                let log = Log.Read
                let previousMessages = JsonConvert.DeserializeObject<StatusMessage list>(log())
                
                // If there are no previous logs, we'll just log the existing messages instead of potentially double posting due to lost state.
                match (box previousMessages = null) with
                | true -> ()
                | false -> 
                    let difference () = (Set.ofList messages) - (Set.ofList previousMessages) |> Set.toList
                    difference () |> List.map payload  |> List.iter postToSlack

                let serialized = JsonConvert.SerializeObject(messages)
                Log.Save serialized
                
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