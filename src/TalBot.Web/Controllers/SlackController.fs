namespace TalBot.Web.Controllers

open System.Configuration
open System.Web.Http
open TalBot

[<CLIMutable>]
type SlackHook = { Token : string;
Team_id : string;
Team_domain : string;
Channel_id : string;
Channel_name : string;
Timestamp : decimal;
User_id : string;
User_name : string;
Text : string;
Trigger_word : string;
};

[<RoutePrefix("api")>]
type SlackController() =
    inherit ApiController()
    [<Route("slack")>]
    member this.Post(slackHook : SlackHook) =        
        let suspiciousIncomingMessage =
            Some{
                token = slackHook.Token
                teamId = slackHook.Team_id
                teamDomain = slackHook.Team_domain
                channelId = slackHook.Channel_id
                channelName = slackHook.Channel_name
                timestamp = slackHook.Timestamp
                userId = slackHook.User_id
                userName = slackHook.User_name
                text = slackHook.Text
                triggerWord = slackHook.Trigger_word
            }

        let bot = Bot(Configuration.botConfiguration(), Configuration.slackConfiguration ())
        let response = bot.Respond suspiciousIncomingMessage
        this.Ok response
