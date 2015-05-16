module TalBot.Agent.Plugins

open System
open System.IO
open System.Reflection
open TalBot

let directoryPath = AppDomain.CurrentDomain.BaseDirectory
// TODO changed path for testing.
//let pluginPath = directoryPath + "\\Plugins"
let pluginPath = @"C:\Workspaces\Personal\TalBot\src\TalBot.Plugins\bin\Debug\"
let files () = Directory.GetFiles(pluginPath,"*.dll") |> Seq.toList

let getPlugins file =
        try
            let a = Assembly.LoadFrom(file)
            [ for t in a.GetTypes() do
                for i in t.GetInterfaces() do
                    if i.Name = "IPlugin" && i.Namespace = "TalBot" then
                        yield a.CreateInstance(t.FullName) ]
        with
        | _ ->
            // if we fail, we'll just skip this assembly.
            List.empty<obj>

let load =
    Directory.CreateDirectory(pluginPath) |> ignore
    // TODO breaking this out from getPlugins to try and figure out why the downcast is failing
    let f = files ()
    let p = List.collect getPlugins (f)
    p |> List.map (fun x ->
        let t = x.GetType()
        let m = t.GetMethods()
        m |> ignore
        printfn "%s" (t.ToString())
        x  :?> IPlugin )