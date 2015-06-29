namespace TalBot

open FSharp.Data

type BotConfiguration = {Name:string;}
type DebugOption = DebugMode | NonDebugMode
type IncomingMessage = 
    {
        token : string;
        teamId : string;
        teamDomain : string;
        channelId : string;
        channelName : string;
        timestamp : decimal;
        userId : string;
        userName : string;
        text : string;
        triggerWord : string;
    }

type JiraConfiguration = {BaseUri:string;TicketCredentials:string;TicketRegex:string;}
type OutgoingMessage = { destination: string; sender: string; text : string; icon: string }
type OutgoingMessageOption = OutgoingMessage option
type Payload = {channel:string; username:string; text:string; icon_emoji:string}
type Response = {text : string; username : string; icon_emoji:string }

type SlackConfiguration =
    {
        DebugChannel:string;        
        ServiceBusReadConnectionString:string;
        ServiceBusWriteConnectionString:string;
        SlackToken:string;
        SlackUri:string;
        WebSocketUri:string;
    }

type SuspiciousIncomingMessage = IncomingMessage option
type Message = JsonProvider<"""{"reply_to":10000,"type":"message","channel":"C04FOOBAR","user":"U04FOOBAR","text":"test-7","ts":"1434721567.000002"}""">