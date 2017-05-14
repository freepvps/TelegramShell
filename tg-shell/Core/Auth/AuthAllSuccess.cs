using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgShell.Core.Auth
{
    public class AuthAllSuccess : AuthBase
    {
        public HashSet<long> AuthSet { get; private set; }
        public AuthAllSuccess()
        {
            AuthSet = new HashSet<long>();
        }
        public override bool CheckAuth(long chatId)
        {
            lock (AuthSet)
            {
                return AuthSet.Contains(chatId);
            }
        }
        public override void Process(Message msg)
        {
            lock (AuthSet)
            {
                AuthSet.Add(msg.Chat.Id);
                TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Auth success").Wait();
            }
        } 
    }
}
