module TalBot.Log

open System.IO
open System

let directoryPath = AppDomain.CurrentDomain.BaseDirectory
let filePath = directoryPath + "\\Log.txt"

let Read() = 
    match FileInfo(filePath).Exists with
    | true -> File.ReadAllText(filePath)
    | false -> ""

let Save lines =
    match Directory.Exists(directoryPath) with
    | true -> ()
    |false -> Directory.CreateDirectory(directoryPath) |> ignore

    File.WriteAllText(filePath, lines)        
