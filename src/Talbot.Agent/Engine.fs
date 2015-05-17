namespace TalBot.Agent

open System
open System.Diagnostics
open System.Threading
open TalBot

type Engine(debugOption:DebugOption) =
    let mutable isRunning = true

    member this.Stop() =
        isRunning <- false

    member this.IsRunning 
        with get() = isRunning
        and set(value) = isRunning <- value

    member this.Run =
        while isRunning do
            try
                Bot.speak debugOption
                
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