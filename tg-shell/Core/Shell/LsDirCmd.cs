using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.IO;

namespace TgShell.Core.Shell
{
    public class LsDirCmd : TgShellCmdHandler
    {
        public override void OnStart(Message msg)
        {
            try
            {
                var args = Helpers.Ext.ParseArgs(msg.Text);

                var path = "./";
                if (args.Length > 1)
                {
                    path = args[1];
                }
                path = Shell.GetPath(path);

                var sb = new StringBuilder();
                sb.AppendLine("Directories:");
                foreach (var dir in Directory.GetDirectories(path))
                {
                    sb.AppendLine("    " + Path.GetFileName(dir));
                }
                sb.AppendLine("Files:");
                foreach (var file in Directory.GetFiles(path))
                {
                    sb.AppendLine("    " + Path.GetFileName(file));
                }

                TelegramApi.SendTextMessageAsync(msg.Chat.Id, sb.ToString());
            }
            catch (Exception ex)
            {
                TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Error: " + ex.Message);
            }
            finally
            {
                Session.Exit(this);
            }
        }
    }
}
