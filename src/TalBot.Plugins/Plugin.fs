module TalBot.Plugins

open System.IO
open TalBot

let directoryInfo () = new DirectoryInfo(@"C:\Test")
let files () = directoryInfo () |> (fun x -> x.GetDirectories("??.??.??.??"))

let changes () = 
    files ()
    |> Seq.sortBy (fun x -> -x.CreationTime.Ticks)
    |> Seq.toList
    |> List.map (fun x -> 
            Some({ StatusMessage.source = "FolderMonitor";
                    message = "Folder " + x.Name + " has been added to the test folder as of " + x.CreationTime.ToString("f") + "." }))

type FolderMonitor() =
    interface IPlugin with
        member x.Run(): StatusMessage option list = 
            changes ()