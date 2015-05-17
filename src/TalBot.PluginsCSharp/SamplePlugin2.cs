using System.Collections.Generic;

namespace TalBot.CSharpPlugins
{
    public class SamplePlugin2 : IPlugin
    {
        public IEnumerable<Message> Run()
        {
            var message = new Message("SlackBot", "SamplePlugin1", "Hello from sample1.", "");
            return new Message[] { message };
        }
    }
}
