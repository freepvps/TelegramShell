using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgShell.Core.Shell
{
    public class ChDirCmd : PathAskCmd
    {
        protected override void ProcessComplete(long chatId)
        {
            try
            {
                var path = Shell.GetPath(Path);
                Shell.CurrentDirectory = path;
                TelegramApi.SendTextMessageAsync(chatId, Shell.CurrentDirectory);
            }
            finally
            {
                Session.Exit(this);
            }
        }
        protected override IEnumerable<string> ListVariants(KeyValuePair<string, string> key)
        {
            yield return ".";
            yield return "..";
            foreach (var dir in Directory.GetDirectories(Shell.CurrentDirectory))
            {
                yield return System.IO.Path.GetFileName(dir);
            }
        }
    }
}
