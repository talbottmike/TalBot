namespace TalBot.IPlugin

open TalBot.Types

type IPlugin =
   abstract member Run: unit -> StatusMessage option list