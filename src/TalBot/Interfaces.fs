namespace TalBot

open System.Collections.Generic

/// Plugins that will generate messages when the bot is asked to speak without being provied input.
/// Can be used by an agent to generate status messages for posting to a channel
type INotificationPlugin =
    /// Returns a collection of messages to post. 
    abstract member Run: unit -> IEnumerable<OutgoingMessage>

/// Plugins that will respond to incoming messages
type IResponsePlugin =
    /// Responds to an incoming message with a collection of outgoing messages.
    /// Can also be used to initiate external behaviour in response to an incoming message.
    abstract member Listen: IncomingMessage -> IEnumerable<OutgoingMessage>