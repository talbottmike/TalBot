module TalBot.PluginLoader

open System
open System.IO
open System.Reflection
open TalBot

let directoryPath = AppDomain.CurrentDomain.BaseDirectory
let pluginPath = directoryPath + "Plugins"
//let pluginPath = @"C:\Workspaces\Personal\TalBot\src\TalBot.PluginsCSharp\bin\Debug\"
//let pluginPath = @"C:\Workspaces\Personal\TalBot\src\TalBot.PluginsFSharp\bin\Debug\"
let files () = Directory.GetFiles(pluginPath,"*.dll") |> Seq.toList

let getPluginsFromFile file =
        try
            let a = Assembly.LoadFrom(file)
            [ for t in a.GetTypes() do
                for i in t.GetInterfaces() do
                    if i.Name = "IPlugin" && i.Namespace = "TalBot" then
                        let pluginObject = a.CreateInstance(t.FullName)
                        match pluginObject with 
                        | :? IPlugin -> 
                            printfn "Valid plugin loaded %s" t.FullName
                            yield a.CreateInstance(t.FullName) :?> IPlugin
                        | _ -> printfn "Invalid plugin %s" t.FullName ]
        with
        | exn ->
            // if we fail, we'll just skip this assembly.
            printfn "Error loading assembly from file %s with exception %s" file exn.Message
            List.empty<IPlugin>

let load =
    Directory.CreateDirectory(pluginPath) |> ignore
    let f = files ()
    List.collect getPluginsFromFile (f)
