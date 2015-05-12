namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("TalBot")>]
[<assembly: AssemblyProductAttribute("TalBot")>]
[<assembly: AssemblyDescriptionAttribute("A bot for posting status messages to Slack and responding to Slack posts.")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
