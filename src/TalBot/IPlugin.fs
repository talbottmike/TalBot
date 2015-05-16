namespace TalBot

type IPlugin =
   abstract member Run: unit -> StatusMessage option list