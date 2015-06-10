using System.Collections.Generic;

namespace TalBot.CSharpPlugins
{
    public class SamplePlugin2 : INotificationPlugin
    {
        public IEnumerable<OutgoingMessage> Run()
        {
            var message = new OutgoingMessage("SlackBot", "SamplePlugin2", "Hello from sample2.", "");
            return new OutgoingMessage[] { message };
        }
    }
}
