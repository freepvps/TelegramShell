using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TgShell.Core.Helpers;

namespace TgShell.Core.Shell
{
    public abstract class PathAskCmd : AskCmd
    {
        public string Path { get; set; } = null;

        protected override KeyValuePair<string, string>? CheckTable()
        {
            if (Path == null)
            {
                return new KeyValuePair<string, string>("path", "Path");
            }
            else
            {
                return null;
            }
        }
        
        protected override void ProcessAnswer(KeyValuePair<string, string> key, string answer)
        {
            Path = Ext.TgNormalizeStr(answer);
        }
        
        protected override void ProcessStartArgs(Message msg, string[] args)
        {
            if (args.Length > 1)
            {
                Path = Ext.TgNormalizeStr(args[1]);
            }
        }
    }
}
