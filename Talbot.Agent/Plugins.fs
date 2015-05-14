module Plugins

open System
open System.IO
open System.Reflection
open TalBot.IPlugin

let directoryPath = AppDomain.CurrentDomain.BaseDirectory
let pluginPath = directoryPath + "\\Plugins"
let files = Directory.GetFiles(pluginPath,"*.dll") |> Seq.toList

let filePlugins =
    (fun file ->
        try
            let a = Assembly.LoadFrom(file)
            [ for t in a.GetTypes() do
                for i in t.GetInterfaces() do
                    if i.Name = "IPlugin" && i.Namespace = "TalBot" then
                        yield a.CreateInstance(t.Namespace + t.Name) :?> IPlugin ]
        with
        | _ ->
            // if we fail, we'll just skip this assembly.
            List.empty<IPlugin>
        )

let load =
    List.collect filePlugins files