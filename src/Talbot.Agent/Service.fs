﻿namespace TalBot.Agent

open System.ServiceProcess
open TalBot
open System.Threading
open TalBot.Configuration

type public Service() =
    inherit ServiceBase(ServiceName = "TalBot")
    let cancellationSource = new CancellationTokenSource()
    let botConfig = Configuration.botConfiguration ()
    let slackConfig = Configuration.slackConfiguration ()
    let bot = Bot(botConfig,slackConfig)

    let botNotifier () = async {
        while true do
            try
                printfn "Asking bot to speak"
                bot.Speak ()
                printfn "Bot done speaking"
                printfn "Sleeping 60 min"          
                do! Async.Sleep 3600000

            with
            | exn -> 
                bot.AttemptToLog exn
                printf "Error: %s" exn.Message
                printfn "Sleeping 2 hr"
                do! Async.Sleep 7200000
        }
                    
    let botResponder () = async {
        bot.Listen ()
        }

    override x.OnStart(args:string[]) = 
        printfn "Starting the bot service"
        printfn "Starting the bot notifier"
        Async.Start(botNotifier (), cancellationSource.Token)
        printfn "Starting the bot responder"
        Async.Start(botResponder (), cancellationSource.Token)
        printfn "The bot service has started."
        base.OnStart(args)

    override x.OnStop() = 
        // Signal the thread to end.
        printfn "Stopping the service."
//        botResponder.Dispose()
        cancellationSource.Cancel()
        base.OnStop()
        printfn "Service Ended"

    override x.OnShutdown() =
        x.OnStop()
        base.OnShutdown()

    member x.debugOnStart() = x.OnStart(null)

    member x.debugOnStop() = x.OnStop()