module TalBot.Agent.Program

open ArgumentParser
open NLog.Config
open NLog.Targets
open NLog
open System
open System.Threading
open System.Diagnostics
open System.Reflection
open System.ServiceProcess
open ServiceHelper
open TalBot
open TalBot.Extensions
open System.Configuration

let getLogger serviceName =
    // Configure logging
    let loggingConfiguration = new LoggingConfiguration()
        
    let consoleTarget = new ColoredConsoleTarget()
    loggingConfiguration.AddTarget("Console", consoleTarget)
    consoleTarget.Layout <- new Layouts.SimpleLayout("${longdate} ${message}")
    loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget))
        
    let eventLogTarget = new EventLogTarget();
    eventLogTarget.Source <- serviceName
    eventLogTarget.Layout <- new Layouts.SimpleLayout("${message}")
    loggingConfiguration.AddTarget("EventLog", eventLogTarget)
    loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, eventLogTarget));

    LogManager.Configuration <- loggingConfiguration
    LogManager.GetLogger(serviceName)

[<EntryPoint>]
let main argv = 
    let args = ArgumentParser.parseCommandLine (argv |> Seq.toList)
    
    let logger = getLogger args.serviceName

    let domainError (sender:obj) (args:UnhandledExceptionEventArgs) =
        let problemDomain = sender :?> AppDomain
        logger.Fatal("Unhandled exception in app domain: " + problemDomain.FriendlyName)
        ()
    
    AppDomain.CurrentDomain.UnhandledException.AddHandler(UnhandledExceptionEventHandler(domainError))
    
    System.AppDomain.CurrentDomain.add_AssemblyResolve ( System.ResolveEventHandler (fun _ args ->
        let resolvedAssembly =
            System.AppDomain.CurrentDomain.GetAssemblies ()
            |> Array.tryFind (fun loadedAssembly ->
                args.Name = loadedAssembly.FullName
                || args.Name = loadedAssembly.GetName().Name)
        
        match resolvedAssembly with
        | Some assembly -> assembly
        | None -> 
            let argName = 
                match args.Name.IndexOf(",") with
                | t when t > 0 -> args.Name.Substring(0, t)
                | _ -> args.Name
            let expectedLocation = System.IO.Path.Combine(PluginLoader.pluginPath, (argName + ".dll" ))
            printfn "Loading assembly from: %s" expectedLocation
            match System.IO.File.Exists expectedLocation with
            | true -> Assembly.LoadFrom expectedLocation
            | false -> null
        ))

    try
        let mutable thread = Thread.CurrentThread
        thread.Name <- args.serviceName

        let installService () = 
            logger.Info("Installing Service " + args.serviceName)
            let applicationPath = Process.GetCurrentProcess().MainModule.FileName
            let installParameters = {installParameters.assemblyPath = applicationPath; serviceName = args.serviceName; displayName = args.serviceName; description="The agent to monitor environment status and post to Slack."; startType=ServiceStartMode.Automatic; userName=""; password=""; dependencies = Array.empty<string> }
            ServiceHelper.installService installParameters
            ServiceHelper.setServiceArguments args.serviceName args.filteredArguments

        let uninstallService () =
            try
                logger.Info("Uninstalling Service " + args.serviceName)
                ServiceHelper.uninstallService args.serviceName
            with
            | exn -> 
                let message = exn.ToDetailedString + " ServiceName: " + args.serviceName
                logger.Fatal(message)

        let debugWait () = 
            while (not Debugger.IsAttached && not Console.KeyAvailable) do
                printfn "Waiting for debugger..."
                Thread.Sleep(250)
            match Console.KeyAvailable with
            | true -> Console.ReadKey() |> ignore
            | false -> ()

        let inDebug = 
            match ConfigurationManager.AppSettings.Item("DebugOption") with
            | "false" -> DebugOption.NonDebugMode
            | _ -> DebugOption.DebugMode

        match inDebug with
        | DebugOption.NonDebugMode -> ()
        | DebugOption.DebugMode -> debugWait()

        let service = new Service()

        let runService () = 
            ServiceBase.Run(service)

        let run () = 
            service.debugOnStart ()
            let mutable running = true
            while (running) do
                match Console.KeyAvailable with
                | false -> Thread.Sleep(50)
                | true -> running <- false

            service.debugOnStop ()

        match args.command with
        | Run -> run ()
        | RunService -> runService ()
        | ShowHelp -> raise (new NotImplementedException("ShowHelp"))
        | InstallService -> 
            logger.Info("Installing service: " + args.serviceName)
            installService()
        | UninstallService ->
            logger.Info("Uninstalling service: " + args.serviceName)
            uninstallService()
    with
        | exn -> 
            logger.Fatal("Exception: " + exn.ToDetailedString, exn)

    0 // return an integer exit code
