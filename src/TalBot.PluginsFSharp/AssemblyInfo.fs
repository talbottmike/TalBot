namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("TalBot.PluginsFSharp")>]
[<assembly: AssemblyProductAttribute("TalBot")>]
[<assembly: AssemblyDescriptionAttribute("A bot for posting status messages to Slack and responding to Slack posts.")>]
[<assembly: AssemblyVersionAttribute("0.0.4")>]
[<assembly: AssemblyFileVersionAttribute("0.0.4")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.4"
