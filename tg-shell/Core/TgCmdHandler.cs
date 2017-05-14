using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgShell.Core
{
    public abstract class TgCmdHandler
    {
        public virtual ITelegramBotClient TelegramApi { get; set; }

        public virtual void Process(Message msg)
        {
            switch (msg.Text)
            {
                case "/help": OnHelp(msg); return;
                case "/cancel": OnCancel(msg); return;
                default: OnMessage(msg); return;
            }
        }

        protected virtual void OnMessage(Message msg)
        {
        }
        protected virtual void OnHelp(Message msg)
        {
            OnMessage(msg);
        }
        protected virtual void OnCancel(Message msg)
        {
        }
    }
}
