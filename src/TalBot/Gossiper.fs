module Gossiper

open FSharp.CloudAgent.Connections
open FSharp.CloudAgent
open TalBot

let spread serviceBusWriteConnectionString incomingMessage =
    /// Post incoming message to service bus queue
    let postToServiceQueue (incomingMessage:IncomingMessage) =
        let serviceBusWriteConnection = ServiceBusConnection serviceBusWriteConnectionString
        let cloudWriteConnection = WorkerCloudConnection(serviceBusWriteConnection, Queue "queue")
        let sendToMessageQueue = ConnectionFactory.SendToWorkerPool cloudWriteConnection
        Async.RunSynchronously (sendToMessageQueue incomingMessage)
    postToServiceQueue incomingMessage