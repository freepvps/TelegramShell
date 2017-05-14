using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot.Types;

using File = System.IO.File;

namespace TgShell.Core.Shell
{
    public class DownloadFileCmd : PathAskCmd
    {
        protected override void ProcessComplete(long chatId)
        {
            var path = Shell.GetPath(Path);
            if (File.Exists(path))
            {
                Task.Run(() =>
                {
                    try
                    {
                        using (var stream = File.OpenRead(path))
                        {
                            var filename = System.IO.Path.GetFileName(path);
                            var fileToSend = new FileToSend(filename, stream);
                            TelegramApi.SendDocumentAsync(chatId, fileToSend).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        TelegramApi.SendTextMessageAsync(chatId, "File sending error: " + ex.Message);
                    }
                    finally
                    {
                        Session.Exit(this);
                    }
                });
            }
            else
            {
                TelegramApi.SendTextMessageAsync(chatId, "File " + path + " not exists");
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
