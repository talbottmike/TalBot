module TalBot.MessageLog

open System
open System.IO

let directoryPath = AppDomain.CurrentDomain.BaseDirectory
let filePath = directoryPath + "\\Log.txt"

// Read existing message log
let Read() = 
    match FileInfo(filePath).Exists with
    | true -> File.ReadAllText(filePath)
    | false -> ""

// Save lines to message log (overwrites existing log)
let Save lines =
    match Directory.Exists(directoryPath) with
    | true -> ()
    |false -> Directory.CreateDirectory(directoryPath) |> ignore

    File.WriteAllText(filePath, lines)        
