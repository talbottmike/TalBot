module TalBot.Agent.Plugins

open System
open System.IO
open System.Reflection
open TalBot

let directoryPath = AppDomain.CurrentDomain.BaseDirectory
// TODO changed path for testing.
//let pluginPath = directoryPath + "\\Plugins"
//let pluginPath = @"C:\Workspaces\Personal\TalBot\src\TalBot.PluginsCSharp\bin\Debug\"
let pluginPath = @"C:\Workspaces\Personal\TalBot\src\TalBot.PluginsFSharp\bin\Debug\"
let files () = Directory.GetFiles(pluginPath,"*.dll") |> Seq.toList

let getPluginsFromFile file =
        try
            let a = Assembly.LoadFrom(file)
            [ for t in a.GetTypes() do
                for i in t.GetInterfaces() do
                    if i.Name = "IPlugin" && i.Namespace = "TalBot" then
                        let blah = a.CreateInstance(t.FullName)
                        match blah with 
                        | :? IPlugin -> printfn "Valid plugin loaded %s" t.FullName
                        | _ -> printfn "Invalid plugin %s" t.FullName

                        yield a.CreateInstance(t.FullName) :?> IPlugin ]
        with
        | exn ->
            // if we fail, we'll just skip this assembly.
            printfn "Error loading assembly from file %s with exception %s" file exn.Message
            List.empty<IPlugin>

let load =
    Directory.CreateDirectory(pluginPath) |> ignore
    // TODO breaking this out from getPlugins to try and figure out why the downcast is failing
    let f = files ()
    let p = List.collect getPluginsFromFile (f)
    p
//    p |> List.map (fun x ->
//        let t = x.GetType()
//        let m = t.GetMethods()
//        m |> ignore
//        printfn "%s" (t.ToString())
//        x  :?> IPlugin )