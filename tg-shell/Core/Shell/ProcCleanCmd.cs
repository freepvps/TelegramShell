using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgShell.Core.Shell
{
    class ProcCleanCmd : TgShellCmdHandler
    {
        public override void OnStart(Message msg)
        {
            try
            {
                Shell.ProcessTable.CleanUp(false);
                TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Clean up complete");
            }
            finally
            {
                Session.Exit(this);
            }
        }
    }
}
