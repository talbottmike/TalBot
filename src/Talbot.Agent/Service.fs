namespace TalBot.Agent

open System.ServiceProcess
open TalBot
open System.Threading
open TalBot.Configuration
open BotHelper

type public Service() =
    inherit ServiceBase(ServiceName = "TalBot")
    let cancellationSource = new CancellationTokenSource()

    let botNotifier = async {
        while true do
            try
                printfn "Asking bot to speak"
                Bot.speak      
                printfn "Bot done speaking"
                printfn "Sleeping 10 min"          
                do! Async.Sleep 600000

            with
            | exn -> 
                Bot.attemptToLog exn
                printf "Error: %s" exn.Message
                printfn "Sleeping 15 min"
                do! Async.Sleep 900000
        }
                    
    let botResponder = Bot.slander ()

    override x.OnStart(args:string[]) = 
        printfn "Starting the bot service"
        printfn "Starting the bot notifier"
        Async.Start(botNotifier, cancellationSource.Token)
        printfn "Starting the bot responder"
        botResponder |> ignore
        printfn "The bot service has started."
        base.OnStart(args)

    override x.OnStop() = 
        // Signal the thread to end.
        printfn "Stopping the service."
        botResponder.Dispose()
        cancellationSource.Cancel()
        base.OnStop()
        printfn "Service Ended"

    override x.OnShutdown() =
        x.OnStop()
        base.OnShutdown()

    member x.debugOnStart() = x.OnStart(null)

    member x.debugOnStop() = x.OnStop()