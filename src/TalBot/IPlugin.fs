namespace TalBot

open System.Collections.Generic

type IPlugin =
   abstract member Run: unit -> IEnumerable<Message>