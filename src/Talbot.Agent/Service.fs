namespace TalBot.Agent

open System.ServiceProcess
open TalBot
open System.Threading

type public Service() =
    inherit ServiceBase(ServiceName = "TalBot")
    let cancellationSource = new CancellationTokenSource()

    let botNotifier = async {
        while true do
            try
                printfn "Asking bot to speak"
                //Bot.speak      
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
        
    let botResponder = async {
        while true do
            try
                printfn "Asking bot to check for gossip"
                Bot.slander ()
                printfn "Bot done slandering"

            with
            | exn -> 
                Bot.attemptToLog exn
                printf "Error: %s" exn.Message
                printfn "Sleeping 10 sec"
                do! Async.Sleep 10000
        }

    override x.OnStart(args:string[]) = 
        printfn "Starting the bot service"
//        printfn "Starting the bot notifier"
//        Async.Start(botNotifier, cancellationSource.Token)
        printfn "Starting the bot responder"
        Async.Start(botResponder, cancellationSource.Token)
        printfn "The bot service has started."
        base.OnStart(args)

    override x.OnStop() = 
        // Signal the thread to end.
        printfn "Stopping the service."
        cancellationSource.Cancel()
        base.OnStop()
        printfn "Service Ended"

    override x.OnShutdown() =
        x.OnStop()
        base.OnShutdown()

    member x.debugOnStart() = x.OnStart(null)

    member x.debugOnStop() = x.OnStop()