using System.Collections.Generic;

namespace TalBot.CSharpPlugins
{
    public class SamplePlugin1 : INotificationPlugin
    {
        IEnumerable<OutgoingMessage> INotificationPlugin.Run()
        {
            var message = new OutgoingMessage("SlackBot", "SamplePlugin1", "Hello from sample1.", "");
            return new OutgoingMessage[] { message };
        }
    }
}
