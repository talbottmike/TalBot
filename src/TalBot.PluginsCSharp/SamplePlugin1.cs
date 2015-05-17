using System.Collections.Generic;

namespace TalBot.CSharpPlugins
{
    public class SamplePlugin1 : IPlugin
    {
        IEnumerable<Message> IPlugin.Run()
        {
            var message = new Message("SlackBot", "SamplePlugin1", "Hello from sample1.", "");
            return new Message[] { message };
        }
    }
}
