﻿namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("TalBot")>]
[<assembly: AssemblyProductAttribute("TalBot")>]
[<assembly: AssemblyDescriptionAttribute("A bot for posting status messages to Slack and responding to Slack posts.")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
