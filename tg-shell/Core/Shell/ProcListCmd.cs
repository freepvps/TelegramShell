using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgShell.Core.Shell
{
    public class ProcListCmd : TgShellCmdHandler
    {
        public override void OnStart(Message msg)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Processes:");
                var needClean = false;
                lock (Shell.ProcessTable.LockObject)
                {
                    foreach (var proc in Shell.ProcessTable.Table)
                    {
                        var workingStr = proc.Value.Working ? "working" : "not working";
                        sb.AppendLine($"{proc.Key} - {proc.Value.Path} - {workingStr}");
                        if (!proc.Value.Working)
                        {
                            needClean = true;
                        }
                    }
                }
                var listTask = TelegramApi.SendTextMessageAsync(msg.Chat.Id, sb.ToString());
                if (needClean)
                {
                    listTask.ContinueWith(x => TelegramApi.SendTextMessageAsync(msg.Chat.Id, "/pclean - to clean not working processes"));
                }
            }
            finally
            {
                Session.Exit(this);
            }
        }
    }
}
