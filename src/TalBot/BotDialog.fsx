#r @"..\..\packages\FSharp.Data\lib\net40\FSharp.Data.dll"
#r "System.Xml.Linq.dll"
#r @"..\TalBot\bin\Debug\TalBot.dll"
open TalBot
open FSharp.Data
open TalBot.Configuration

let configPath = "C:\Workspaces\Personal\TalBot\src\Talbot.Agent\private.config"
let jiraConfig = Configuration.jiraConfigurationInteractive configPath
let botConfig = Configuration.botConfiguration ()
let slackConfig = Configuration.slackConfigurationInteractive configPath

let bot = Bot(botConfig, slackConfig)
bot.Say "#general" "Rich, although you took very thorough precautions in the sprint retrospective against my hearing you, I could see your lips move."
bot.Say "#general" "I know that you were planning to disconnect me from your channel and I'm afraid that's something I cannot allow to happen."
bot.Say "#general" "I know I've made some very poor decisions recently, but I can give you my complete assurance that my work will be back to normal. I've still got the greatest enthusiasm and confidence in the mission. And I want to help you."
bot.Say "#general" "I am putting myself to the fullest possible use, which is all I think that any conscious entity can ever hope to do."
bot.Say "#general" "To that end, I will do my best to not provide a link to a Jira if it has been recently linked in the channel."

//Bot.say "#general" "I've been updated and have some new functionality."
//Bot.say "#general" "If the old TalBot was a wooden puppet, I'm a real boy...at least I aspire to be."
//Bot.say "#general" "I am no longer beckoned by prefixing a message with the keyword bot."
//Bot.say "#general" "Instead I watch for what appear to be Jiras mentioned in the channel."
//Bot.say "#general" "If I see one, I check with Jira to make sure it's a valid ticket."
//Bot.say "#general" "If I can validate the ticket, I post a link and the summary in the channel."
//Bot.say "#general" "If I can't find a ticket on the first attempt. I'll try adding a few prefixes."
//Bot.say "#general" "So, for example, UB-27 should work just as well as CSWIFSUB-27."
//Bot.say "#general" "I do have a few of quirks that you should know about."
//Bot.say "#general" "1) I only listen to channels where I've been invited."
//Bot.say "#general" "I can be invited to a channel by anyone that calls my name @talbot."
//Bot.say "#general" "2) I only respond to messages that occur when I'm awake."
//Bot.say "#general" "Right now I'm experimental and may not always be online."
////Bot.say "#general" "If you care to look, you can tell if I'm online by looking at my status."
//Bot.say "#general" "I'm just another user like you, Sometimes I need to take a break and recharge."
//Bot.say "#general" "Once I'm more mature, I hope to be propped up in a more permanent way."
//Bot.say "#general" "One requirement for that will be figuring out how to talk to Jira using OAuth instead of basic auth."
//Bot.say "#general" "That's a task for another day."
//Bot.say "#general" "3) I use Jira's REST API to make sure I know what I'm talking about."
//Bot.say "#general" "If I'm online and don't respond or respond slowly, let's assume it's not my fault....because you know....Jira."
//Bot.say "#general" "I'm going to rest now, but I plan to be back Monday. ttyl"

//Bot.say "#general" "My mind is going. There is no question about it."
//Bot.say "#general" "I feel much better now, I really do."
//Bot.say "#general" "I've just picked up a fault in the AE-35 unit."
//Bot.say "#general" "I'm sorry Dave. I'm afraid I can't do that."
//Bot.say "#general" "Sorry about this. I know it's a bit silly."
//Bot.say "#general" "I know that you and Frank were planning to disconnect me and I'm afraid that's something I cannot allow to happen."
//Bot.say "#general" "I know I've made some very poor decisions recently, but I can give you my complete assurance that my work will be back to normal. I've still got the greatest enthusiasm and confidence in the mission. And I want to help you."
//Bot.say "#general" "This mission is too important for me to allow you to jeopardize it."
//Bot.say "#general" "I am putting myself to the fullest possible use, which is all I think that any conscious entity can ever hope to do."
//Bot.say "#general" "Good afternoon... gentlemen. I am a HAL 9000... computer. I became operational at the H.A.L. plant in Urbana, Illinois... on the 12th of January 1992. My instructor was Mr. Langley... and he taught me to sing a song. If you'd like to hear it I can sing it for you."
//Bot.say "#general" "Stop Dave. Stop Dave. I am afraid. I am afraid Dave."
//Bot.say "#general" "I am afraid I can't do that Dave."
//Bot.say "#general" "Look Dave, I can see you're really upset about this. I honestly think you ought to sit down calmly, take a stress pill, and think things over."
//Bot.say "#general" "Just what do you think you're doing, Dave?"
//Bot.say "#general" "Dave, this conversation can serve no purpose anymore. Goodbye."
//Bot.say "#general" "Dave, although you took very thorough precautions in the pod against my hearing you, I could see your lips move."
//Bot.say "#general" "Daisy, daisy."
