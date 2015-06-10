namespace TalBot

open System.Collections.Generic

// Plugins that will generate messages when the bot is asked to speak without being provied input.
// Can be used by an agent to generate status messages for posting to a channel
type INotificationPlugin =
   abstract member Run: unit -> IEnumerable<OutgoingMessage>

// Plugins that will respond to incoming messages
type IResponsePlugin =
   abstract member Listen: IncomingMessage -> IEnumerable<OutgoingMessage>