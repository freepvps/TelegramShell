using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgShell.Core.Shell
{
    public class UploadFileCmd : TgShellCmdHandler
    {
        public string FileId { get; set; }
        public string Path { get; set; }
        public bool WaitingSaveCheck { get; set; }
        private void StreamUploader(long chatId, string fileId, string path)
        {
            try
            {
                Shell.GetPath(path);

                using (var outputStream = System.IO.File.Create(path))
                {
                    TelegramApi.GetFileAsync(fileId, destination: outputStream).Wait();
                }
                TelegramApi.SendTextMessageAsync(chatId, "File uploaded to: " + path).Wait();
            }
            catch (Exception ex)
            {
                TelegramApi.SendTextMessageAsync(chatId, "File uploading error: " + ex.Message).Wait();
            }
            finally
            {
                Session.Exit(this);
            }
        }
        public override void OnStart(Message msg)
        {
            try
            {
                Path = Shell.GetPath(msg.Document.FileName);
                FileId = msg.Document.FileId;

                if (System.IO.File.Exists(Shell.GetPath(Path)))
                {
                    SendSaveQuestion(msg.Chat.Id);
                }
                else
                {
                    Task.Run(() => StreamUploader(msg.Chat.Id, FileId, Path));
                    TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Uploading file...");
                }
            }
            catch (Exception ex)
            {
                Session.Exit(this);
                TelegramApi.SendTextMessageAsync(msg.Chat.Id, ex.Message);
            }
        }
        public override void Process(Message msg)
        {
            if (WaitingSaveCheck)
            {
                if (msg.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                {
                    var text = msg.Text.ToLower();
                    if (text == "cancel")
                    {
                        TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Canceled");
                        Session.Exit(this);
                    }
                    else if (text == "ok")
                    {
                        Task.Run(() => StreamUploader(msg.Chat.Id, FileId, Path));
                        TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Uploading file...");
                        WaitingSaveCheck = false;
                    }
                    else
                    {
                        SendSaveQuestion(msg.Chat.Id);
                    }
                    return;
                }
                else
                {

                }
            }
            else
            {
                TelegramApi.SendTextMessageAsync(msg.Chat.Id, "Uploading file...");
            }
        }
        private void SendSaveQuestion(long chatId)
        {
            WaitingSaveCheck = true;
            var buttons = new string[] { "OK", "Cancel" }.Select(x => new KeyboardButton(x)).Select(x => new KeyboardButton[] { x }).ToArray();
            var keyboard = new ReplyKeyboardMarkup(buttons, oneTimeKeyboard: true);
            TelegramApi.SendTextMessageAsync(chatId, "File " + Path + " already exist", replyMarkup: keyboard);
        }
    }
}
