module TalBot.PluginLoader

open System
open System.IO
open System.Reflection
open TalBot
open TalBot.Extensions

// Application directory
let directoryPath = AppDomain.CurrentDomain.BaseDirectory

// Path of plugins to load
let pluginPath = directoryPath + "Plugins"

// Gets instances of the implemented notification plugins from the assembly file
let getNotificationPlugins assemblyName =
        try
            printfn "checking assembly for plugins: %s" assemblyName
            let assembly = Assembly.Load(assemblyString = assemblyName)
            [ for t in assembly.GetTypes() do
                for i in t.GetInterfaces() do
                    if i.Name = "INotificationPlugin" && i.Namespace = "TalBot" then
                        let pluginObject = assembly.CreateInstance(t.FullName)
                        match pluginObject with 
                        | :? INotificationPlugin -> 
                            printfn "Valid plugin loaded %s" t.FullName
                            yield pluginObject :?> INotificationPlugin
                        // TODO check if the item created is disposable and displose if it is.
                        | _ -> printfn "Invalid plugin %s" t.FullName ]
        with
        | exn ->
            // if we fail, we'll just skip this assembly.
            printfn "Error loading assembly from file %s with exception %s" assemblyName exn.ToDetailedString
            List.empty<INotificationPlugin>

// Gets a collection of notification plugins
let loadNotificationPlugins () =
    Directory.CreateDirectory(pluginPath) |> ignore
    let files = 
        Directory.GetFiles(pluginPath,"*.dll")
        |> Seq.map (fun x -> Path.GetFileNameWithoutExtension(x))
    Seq.collect getNotificationPlugins (files)

// Gets instances of the implemented response plugins from the assembly file
let getResponsePlugins assemblyName =
        try
            let assembly = Assembly.Load(assemblyString = assemblyName)
            [ for t in assembly.GetTypes() do
                for i in t.GetInterfaces() do
                    if i.Name = "IResponsePlugin" && i.Namespace = "TalBot" then
                        let pluginObject = assembly.CreateInstance(t.FullName)
                        match pluginObject with 
                        | :? IResponsePlugin -> 
                            printfn "Valid plugin loaded %s" t.FullName
                            yield pluginObject :?> IResponsePlugin
                        // TODO check if the item created is disposable and displose if it is.
                        | _ -> printfn "Invalid plugin %s" t.FullName ]
        with
        | exn ->
            // if we fail, we'll just skip this assembly.
            printfn "Error loading assembly from file %s with exception %s" assemblyName exn.ToDetailedString
            List.empty<IResponsePlugin>

// Gets a collection of response plugins
let loadResponsePlugins () =
    Directory.CreateDirectory(pluginPath) |> ignore
    let files = 
        Directory.GetFiles(pluginPath,"*.dll")
        |> Seq.map (fun x -> Path.GetFileNameWithoutExtension(x))
    Seq.collect getResponsePlugins (files)