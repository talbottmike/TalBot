namespace TalBot.Agent

open System.ServiceProcess
open System.Threading.Tasks
open System

type public Service(bot:Bot) =
    inherit ServiceBase(ServiceName = "SlackBot")
    let mutable serviceTask : Task = null

    member this.IsRunning 
        with get() = bot.IsRunning

    override x.OnStart(args:string[]) = 
        printfn "Starting the service."
        serviceTask <- System.Threading.Tasks.Task.Run(fun _ -> bot.Run) 
        printfn "The service has started."
        base.OnStart(args)

    override x.OnStop() = 
        // Signal the thread to end.
        printfn "Stopping the service."
        bot.Stop()

        // Wait one minute for the thread to end.
        match serviceTask.Wait(new TimeSpan(0, 1, 0)) with
        | true -> printfn "The service has stopped."
        | false -> printfn "The service has stopped without completion."

        base.OnStop()
        printfn "Service Ended"

    override x.OnShutdown() =
        x.OnStop()
        base.OnShutdown()

    member x.debugOnStart() = x.OnStart(null)

    member x.debugOnStop() = x.OnStop()