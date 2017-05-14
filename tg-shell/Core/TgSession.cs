using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types;

using TgShell.Core.Auth;

namespace TgShell.Core
{
    public class TgSession : TgCmdHandler
    {
        public AuthBase AuthHandler { get; set; }
        public TgSessionCmdHandler MainHandler { get; set; }
        private Stack<TgSessionCmdHandler> HandlersStack { get; set; }

        public TgSession(ITelegramBotClient telegramApi, AuthBase authHandler, TgSessionCmdHandler mainHandler)
        {
            TelegramApi = telegramApi;
            AuthHandler = authHandler;
            MainHandler = mainHandler;

            HandlersStack = new Stack<TgSessionCmdHandler>();

            Initialize(mainHandler);
        }

        public T StartHandler<T>(Message msg = null) where T : TgSessionCmdHandler, new()
        {
            return StartHandler<T>(new T(), msg);
        }
        public T StartHandler<T>(T tgCmdHandler, Message msg = null) where T : TgSessionCmdHandler
        {
            StartHandler((TgSessionCmdHandler)tgCmdHandler, msg);
            return tgCmdHandler;
        }
        public TgSessionCmdHandler StartHandler(TgSessionCmdHandler tgCmdHandler, Message msg = null)
        {
            lock (HandlersStack)
            {
                HandlersStack.Push(tgCmdHandler);
                Initialize(tgCmdHandler);
                if (msg != null)
                {
                    tgCmdHandler.OnStart(msg);
                }
            }
            return tgCmdHandler;
        }
        public void Exit(TgSessionCmdHandler tgCmdHandler)
        {
            lock (HandlersStack)
            {
                if (HandlersStack.Contains(tgCmdHandler))
                {
                    while (HandlersStack.Count > 0)
                    {
                        var last = HandlersStack.Pop();
                        if (last == tgCmdHandler)
                        {
                            break;
                        }
                    }
                }
            }
        }

        protected void Initialize(TgSessionCmdHandler tgCmdHandler)
        {
            tgCmdHandler.Session = this;
            tgCmdHandler.TelegramApi = TelegramApi;
            tgCmdHandler.Initialize();
        }
        
        public override void Process(Message msg)
        {
            if (AuthHandler.CheckAuth(msg.Chat.Id))
            {
                if (HandlersStack.Count > 0)
                {
                    HandlersStack.Peek().Process(msg);
                }
                else
                {
                    MainHandler.Process(msg);
                }
            }
            else
            {
                AuthHandler.Process(msg);
            }
        }
    }
}
