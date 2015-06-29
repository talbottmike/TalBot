namespace TalBot

open BotHelper
open TalBot
open FSharp.CloudAgent.Connections
open FSharp.CloudAgent

type Slanderer(slackUri)=

    /// Listen for incoming message from the service bus queue and process using the provided agent
    member this.Slander serviceBusReadConnectionString =
        // Create an Agent on demand to load response plugins and process incoming messages.
        let createAgent agentId =
            MailboxProcessor.Start(fun inbox ->
                async {
                    while true do
                        let! message = inbox.Receive()
                        let responsePlugins = PluginLoader.loadResponsePlugins ()

                        let responsePluginResult (plugin:IResponsePlugin) = 
                            plugin.Listen message |> Seq.map (fun x -> Some(x))

                        let responsePluginResults =
                            Seq.collect responsePluginResult responsePlugins

                        let slack = Slack.create slackUri
                        responsePluginResults |> Seq.choose (fun x -> x) |> Seq.map buildPayload |> Seq.iter (fun x -> Slack.post x slack)     
                })
        // Listen for incoming message from the service bus queue and process using the provided agent
        let readFromServiceQueue serviceBusReadConnectionString =
            let serviceBusReadConnection = ServiceBusConnection serviceBusReadConnectionString
            let cloudReadConnection = WorkerCloudConnection(serviceBusReadConnection, Queue "queue")
            ConnectionFactory.StartListening(cloudReadConnection, createAgent >> Messaging.CloudAgentKind.BasicCloudAgent)

        readFromServiceQueue serviceBusReadConnectionString