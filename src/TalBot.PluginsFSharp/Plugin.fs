namespace TalBot.PluginsFSharp

open System.IO
open TalBot
open System.Collections.Generic

type FolderMonitorSample() =
    interface IPlugin with
        member x.Run(): IEnumerable<StatusMessage> =    
            [|{StatusMessage.source="SamplePluginFSharp"; message="Hello from FSharp plug-in"}|] :> seq<StatusMessage>
//            let directoryInfo () = new DirectoryInfo(@"C:\Test")
//            let files () = directoryInfo () |> (fun x -> x.GetDirectories(""))
//
//            let changes () = 
//                files ()
//                |> Seq.sortBy (fun x -> -x.CreationTime.Ticks)
//                |> Seq.map (fun x -> 
//                        { StatusMessage.source = "FolderMonitor";
//                                message = "Folder " + x.Name + " has been added to the test folder as of " + x.CreationTime.ToString("f") + "." })
//            changes ()