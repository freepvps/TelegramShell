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
    public abstract class AskCmd : TgShellCmdHandler
    {

        public KeyValuePair<string, string>? TempKey { get; protected set; } = null;
        public override void OnStart(Message msg)
        {
            try
            {
                var args = Helpers.Ext.ParseArgs(msg.Text);
                ProcessStartArgs(msg, args);
                ProcessNext(msg.Chat.Id);
            }
            catch (Exception ex)
            {
                Session.Exit(this);
                TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Error: " + ex.Message);
            }
            finally
            {
            }
        }
        public override void Process(Message msg)
        {
            if (string.IsNullOrWhiteSpace(msg.Text))
            {
                if (TempKey == null)
                {
                    TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Waiting..");
                }
                else
                {
                    SendQuestion(msg.Chat.Id, TempKey.Value);
                }
            }
            else
            {
                if (TempKey == null)
                {
                    if (msg.Text == "/cancel")
                    {
                        Session.Exit(this);
                    }
                    else
                    {
                        TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Waiting..");
                    }
                }
                else
                {
                    ProcessAnswer(TempKey.Value, msg.Text);
                    TempKey = null;
                    ProcessNext(msg.Chat.Id);
                }
            }
        }
        protected virtual void ProcessNext(long chatId)
        {
            var key = CheckTable();
            if (key == null)
            {
                ProcessComplete(chatId);
            }
            else
            {
                TempKey = key;
                SendQuestion(chatId, key.Value);
            }
        }
        protected virtual void SendQuestion(long chatId, KeyValuePair<string, string> key)
        {
            var variants = ListVariants(key);
            var buttons = variants.Select(x => new KeyboardButton(x)).Select(x => new KeyboardButton[] { x }).ToArray();
            var keyboard = new ReplyKeyboardMarkup(buttons, oneTimeKeyboard: true);
            TelegramApi.SendTextMessageAsync(chatId, key.Value, replyMarkup: keyboard);
        }


        protected abstract void ProcessAnswer(KeyValuePair<string, string> key, string answer);
        protected abstract void ProcessStartArgs(Message msg, string[] args);
        protected abstract KeyValuePair<string, string>? CheckTable();
        protected abstract IEnumerable<string> ListVariants(KeyValuePair<string, string> key);
        protected abstract void ProcessComplete(long chatId);
    }
}
