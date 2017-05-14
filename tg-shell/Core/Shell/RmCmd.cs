using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TgShell.Core.Shell
{
    public class RmCmd : PathAskCmd
    {
        protected override void ProcessComplete(long chatId)
        {
            try
            {
                var path = Shell.GetPath(Path);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    TelegramApi.SendTextMessageAsync(chatId, "File " + path + " removed");
                }
                else
                {
                    TelegramApi.SendTextMessageAsync(chatId, "File " + path + " not exists");
                }
            }
            finally
            {
                Session.Exit(this);
            }
        }
        protected override IEnumerable<string> ListVariants(KeyValuePair<string, string> key)
        {
            foreach (var dir in Directory.GetFiles(Shell.CurrentDirectory))
            {
                yield return System.IO.Path.GetFileName(dir);
            }
        }
    }
}
