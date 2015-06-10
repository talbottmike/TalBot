module TalBot.PluginLoader

open System
open System.IO
open System.Reflection
open TalBot

// Path of the running executable
let directoryPath = AppDomain.CurrentDomain.BaseDirectory

// Path of plugins to load
let pluginPath = directoryPath + "Plugins"

// Gets assembly files that implement the plugin interface
let files path = 
    let loadedAssemblies = 
        AppDomain.CurrentDomain.GetAssemblies() 
        |> Seq.map (fun x -> x.FullName.Split(',') |> Seq.head)
    let foundAssemblyFiles = 
        Directory.GetFiles(path,"*.dll") 
    let nonLoadedAssemblyFiles =
        foundAssemblyFiles
        |> Seq.filter (fun x -> not (Seq.exists ((=) (Path.GetFileNameWithoutExtension(x))) loadedAssemblies)) 
        |> Seq.toList
    nonLoadedAssemblyFiles

// Gets instances of the implemented plugins from the assembly file
let getPluginsFromFile file =
        try
            let a = Assembly.LoadFrom(file)
            [ for t in a.GetTypes() do
                for i in t.GetInterfaces() do
                    if i.Name = "IPlugin" && i.Namespace = "TalBot" then
                        let pluginObject = a.CreateInstance(t.FullName)
                        match pluginObject with 
                        | :? INotificationPlugin -> 
                            printfn "Valid plugin loaded %s" t.FullName
                            yield a.CreateInstance(t.FullName) :?> INotificationPlugin
                        | _ -> printfn "Invalid plugin %s" t.FullName ]
        with
        | exn ->
            // if we fail, we'll just skip this assembly.
            printfn "Error loading assembly from file %s with exception %s" file exn.Message
            List.empty<INotificationPlugin>

// Gets a collection of plugins
let load =
    Directory.CreateDirectory(pluginPath) |> ignore
    let f = files pluginPath
    List.collect getPluginsFromFile (f)
