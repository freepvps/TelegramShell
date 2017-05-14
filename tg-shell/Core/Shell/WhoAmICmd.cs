using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgShell.Core.Shell
{
    public class WhoAmICmd : TgShellCmdHandler
    {
        public override void OnStart(Message msg)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Chat id: {msg.Chat.Id}");
                sb.AppendLine($"Chat first name: {msg.Chat.FirstName}");
                sb.AppendLine($"Chat last name: {msg.Chat.LastName}");
                sb.AppendLine($"Chat user name: {msg.Chat.Username}");
                sb.AppendLine($"Chat title: {msg.Chat.Title}");

                TelegramApi.SendTextMessageAsync(msg.Chat.Id, sb.ToString());
            }
            finally
            {
                Session.Exit(this);
            }
        }
    }
}
