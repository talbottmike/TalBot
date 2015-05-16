using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace TalBot.CSharpPlugins
{
    public class Class1 : IPlugin
    {
        FSharpList<FSharpOption<StatusMessage>> IPlugin.Run()
        {
            throw new NotImplementedException();
        }
    }
}
