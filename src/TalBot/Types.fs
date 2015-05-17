namespace TalBot

type OutgoingMessage = { destination: string; sender: string; text : string; icon: string }
type OutgoingMessageOption = OutgoingMessage option
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

type Payload = {channel:string; username:string; text:string; icon_emoji:string}
type Response = {text : string; username : string; icon_emoji:string }