using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgShell.Core.Auth
{
    class AuthAllFail : AuthBase
    {
        public override bool CheckAuth(long chatId)
        {
            return false;
        }
        public override void Process(Message msg)
        {
            TelegramApi.SendTextMessageAsync(msg.Text, "Auth failed");
        }
    }
}
