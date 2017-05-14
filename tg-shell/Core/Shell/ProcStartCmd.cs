using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot.Types;
using TgShell.Core.Helpers;

namespace TgShell.Core.Shell
{
    public class ProcStartCmd : AskCmd
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }

        protected override void ProcessAnswer(KeyValuePair<string, string> key, string answer)
        {
            switch (key.Key)
            {
                case "name": Name = answer; return;
                case "cmd": Command = answer; return;
                case "args": Arguments = answer; return;
            }
        }

        protected override void ProcessStartArgs(Message msg, string[] args)
        {
            if (args.Length > 1) Name = args[1];
            if (args.Length > 2) Command = args[2];
            if (args.Length > 3)
            {
                var text = msg.Text;
                var offset = Helpers.Ext.GetEndOffset(text, 2);
                if (offset != -1)
                {
                    Arguments = text.Substring(offset).TrimStart();
                    Arguments = Ext.TgNormalizeStr(Arguments);
                }
            }
        }

        protected override KeyValuePair<string, string>? CheckTable()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new KeyValuePair<string, string>("name", "Enter proc id");
            }
            if (string.IsNullOrWhiteSpace(Command))
            {
                if (Shell.ProcessTable.Take(Name) != null)
                {
                    return null;
                }
                return new KeyValuePair<string, string>("cmd", "Enter command");
            }
            return null;
        }

        protected override IEnumerable<string> ListVariants(KeyValuePair<string, string> key)
        {
            if (key.Key == "cmd")
            {
                foreach (var dir in Directory.GetFiles(Shell.CurrentDirectory))
                {
                    yield return System.IO.Path.GetFileName(dir);
                }
            }
            else if (key.Key == "name")
            {
                for (var i = 1; ; i++)
                {
                    if (Shell.ProcessTable.Take(i.ToString()) == null)
                    {
                        yield return i.ToString();
                        break;
                    }
                }
            }
        }

        protected override void ProcessComplete(long chatId)
        {
            if (string.IsNullOrWhiteSpace(Command))
            {
                Session.Exit(this);
                Shell.ProcessTable.Attach(Name);
            }
            else
            {
                Session.Exit(this);
                var exec = Shell.ProcessTable.Register(Name, Command, Arguments);
                Shell.ProcessTable.Attach(Name);
                exec.InitProcess(chatId);
            }
        }
    }
}
