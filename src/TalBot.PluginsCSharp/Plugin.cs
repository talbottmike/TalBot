using System.Collections.Generic;

namespace TalBot.CSharpPlugins
{
    public class Sample1 : IPlugin
    {
        IEnumerable<StatusMessage> IPlugin.Run()
        {
            var message = new StatusMessage("SamplePlugin1", "Hello from sample1.");
            return new StatusMessage[] { message };
        }
    }

    public class Sample2 : IPlugin
    {
        public IEnumerable<StatusMessage> Run()
        {
            var message = new StatusMessage("SamplePlugin2", "Hello from sample1.");
            return new StatusMessage[] { message };
        }
    }
}
