using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.IO;
using Telegram.Bot.Types.Enums;

namespace TgShell.Core.Shell
{
    public class TgShellSession : TgSessionCmdHandler
    {
        public ProcessTable ProcessTable { get; private set; }
        public string CurrentDirectory { get; set; }

        public TgShellSession()
        {
            ProcessTable = new ProcessTable(this);
        }

        public string GetPath(string path)
        {
            return Path.GetFullPath(Path.Combine(CurrentDirectory, path));
        }

        public T StartHandler<T>(Message msg = null) where T : TgShellCmdHandler, new()
        {
            return StartHandler<T>(new T(), msg);
        }
        public T StartHandler<T>(T cmdHandler, Message msg = null) where T : TgShellCmdHandler
        {
            StartHandler((TgShellCmdHandler)cmdHandler, msg);
            return cmdHandler;
        }
        public TgShellCmdHandler StartHandler(TgShellCmdHandler shellCmdHandler, Message msg = null)
        {
            shellCmdHandler.Shell = this;
            Session.StartHandler(shellCmdHandler, msg);
            return shellCmdHandler;
        }
        public override void Initialize()
        {
            CurrentDirectory = Environment.CurrentDirectory;
        }

        protected override void OnHelp(Message msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"TgShell help:");
            sb.AppendLine($"    /cd - change dir");
            sb.AppendLine($"    /ls - list dir");
            sb.AppendLine($"    /cat - cat file");
            sb.AppendLine($"    /rm - remove file");
            sb.AppendLine($"    /download - download file from dir");
            sb.AppendLine($"    /exec - start a process");
            sb.AppendLine($"    /pls - list session's processes");
            sb.AppendLine($"    /whoami - info about this chat");
            sb.AppendLine($"Current directory:");
            sb.AppendLine($"    {CurrentDirectory}");

            TelegramApi.SendTextMessageAsync(msg.Chat.Id, sb.ToString()).Wait();
        }
        public override void Process(Message msg)
        {
            if (msg.Type == MessageType.TextMessage)
            {
                var arg = Helpers.Ext.GetArg(msg.Text, 0);
                switch (arg)
                {
                    case "/ls": StartHandler<LsDirCmd>(msg); break;
                    case "/cd": StartHandler<ChDirCmd>(msg); break;
                    case "/cat": StartHandler<CatCmd>(msg); break;
                    case "/rm": StartHandler<RmCmd>(msg); break;
                    case "/download": StartHandler<DownloadFileCmd>(msg); break;
                    case "/exec": StartHandler<ProcStartCmd>(msg); break;
                    case "/whoami": StartHandler<WhoAmICmd>(msg); break;
                    case "/pls": StartHandler<ProcListCmd>(msg); break;
                    case "/pclean": StartHandler<ProcCleanCmd>(msg); break;
                    default: OnHelp(msg); break;
                }
            }
            else if (msg.Type == MessageType.DocumentMessage)
            {
                StartHandler<UploadFileCmd>(msg);
            }
        }
    }
}
