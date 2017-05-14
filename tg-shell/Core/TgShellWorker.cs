using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using TgShell.Core.Auth;
using TgShell.Core.Shell;

namespace TgShell.Core
{
    public class TgShellWorker
    {
        private object workingLock = new object();
        private object configLock = new object();

        public bool Started { get; private set; }
        public Config Config { get; private set; }


        public Dictionary<long, TgSession> SessionsTable { get; private set; }

        public TgShellWorker()
        {
            SessionsTable = new Dictionary<long, TgSession>();
        }

        public ITelegramBotClient TelegramApi { get; private set; }
        private int UpdateId = 0;

        public void Configure(Config config)
        {
            lock (configLock)
            {
                Config = config;
                TelegramApi = new TelegramBotClient(config.ApiKey);
                UpdateId = 0;
                SessionsTable.Clear();
            }
        }

        private TgSession CreateSession()
        {
            AuthBase auth;
            lock (configLock)
            {
                switch (Config.AuthType)
                {
                    case "successAll": auth = new AuthAllSuccess(); break;
                    default: auth = new AuthAllFail(); break;
                }
            }
            auth.TelegramApi = TelegramApi;
            return new TgSession(TelegramApi, auth, new TgShellSession());
        }

        private Task WorkTask;
        private CancellationTokenSource TokenSource;
        public void Start()
        {
            lock (workingLock)
            {
                if (Started)
                {
                    return;
                }
                TokenSource = new CancellationTokenSource();
                WorkTask = Task.Run((Func<Task>)Working, TokenSource.Token);
                Started = true;
            }
        }
        public void Stop()
        {
            lock (workingLock)
            {
                if (!Started)
                {
                    return;
                }

                TokenSource.Cancel();
                WorkTask.Dispose();
                WorkTask = null;
                TokenSource = null;
                Started = false;
            }
        }
        private async Task Working()
        {
            while (true)
            {
                try
                {
                    ITelegramBotClient api;
                    int updateId;
                    lock (configLock)
                    {
                        api = TelegramApi;
                        updateId = this.UpdateId;
                    }
                    foreach (var update in await api.GetUpdatesAsync(updateId))
                    {
                        updateId = update.Id + 1;
                        if (update.Message == null)
                        {
                            continue;
                        }
                        Console.WriteLine($"{updateId - 1} : {update.Message.Chat.Id} {update.Message.Chat.Username} {update.Message.Text}");
                        TgSession session;
                        lock (configLock)
                        {
                            if (!SessionsTable.TryGetValue(update.Message.Chat.Id, out session))
                            {
                                session = CreateSession();
                                SessionsTable[update.Message.Chat.Id] = session;
                            }
                        }
                        if (session == null)
                        {
                            continue;
                        };
                        try
                        {
                            session.Process(update.Message);
                        }
                        catch
                        {

                        }
                    }
                    lock (configLock)
                    {
                        if (TelegramApi == api)
                        {
                            UpdateId = updateId;
                        }
                    }
                }
                catch
                {

                }
                await Task.Delay(100);
            }
        }

    }
}
