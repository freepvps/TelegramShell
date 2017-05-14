using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.IO;
using System.Diagnostics;
using TgShell.Core.Helpers;
using System.Threading;

using File = System.IO.File;

namespace TgShell.Core.Shell
{
    public class ExecCmd : PathAskCmd
    {
        const string CancelCmd = "/cancel";
        const string DetachCmd = "/detach";

        public Process ExecProcess { get; private set; }
        public string Arguments { get;  set; }
        public bool Autoflush { get; set; }
        public int SleepInterval { get; set; } = 100;
        public int BufferSize { get; set; } = 4096;

        private StringBuilder Output { get; set; } = new StringBuilder();

        public bool Started
        {
            get
            {
                return ExecProcess != null;
            }
        }
        public bool Working
        {
            get
            {
                return Started && !ExecProcess.HasExited;
            }
        }

        public override void Process(Message msg)
        {
            if (ExecProcess == null)
            {
                base.Process(msg);
            }
            else
            {
                if (ExecProcess.HasExited)
                {
                    Exit();
                    return;
                }
                if (msg.Type == MessageType.TextMessage)
                {
                    var text = msg.Text;
                    text = Ext.TgNormalizeStr(text);

                    switch (text)
                    {
                        case CancelCmd: Exit(); return;
                        case DetachCmd: Detach(); return;
                        default:
                            text = Ext.ExpandCmd(text, CancelCmd);
                            text = Ext.ExpandCmd(text, DetachCmd);
                            break;
                    }

                    ExecProcess.StandardInput.WriteLineAsync(text).Wait();
                    ExecProcess.StandardInput.Flush();
                }
            }
        }
        protected override void ProcessStartArgs(Message msg, string[] args)
        {
            base.ProcessStartArgs(msg, args);
            if (args.Length > 1)
            {
                var text = msg.Text;
                var offset = Helpers.Ext.GetEndOffset(text, 1);
                if (offset != -1)
                {
                    Arguments = text.Substring(offset).TrimStart();
                    Arguments = Ext.TgNormalizeStr(Arguments);
                }
            }
        }

        public virtual void Detach()
        {
            Autoflush = false;
            Session.Exit(this);
        }
        public virtual void Exit()
        {
            try
            {
                ExecProcess?.Close();
                ExecProcess?.Dispose();
            }
            catch
            {

            }
            finally
            {
                ExecProcess = null;
                Detach();
            }
        }
        protected override IEnumerable<string> ListVariants(KeyValuePair<string, string> key)
        {
            foreach (var dir in Directory.GetFiles(Shell.CurrentDirectory))
            {
                yield return System.IO.Path.GetFileName(dir);
            }
        }
        
        private void BeginProxy(long chatId, params StreamReader[] readers)
        {
            try
            {
                var tasks = new Task<string>[readers.Length];
                for (var i = 0; i < readers.Length; i++)
                {
                    tasks[i] = readers[i].ReadLineAsync();
                }
                var sb = new StringBuilder();
                while (ExecProcess != null)
                {
                    if (ExecProcess.HasExited)
                    {
                        Exit();
                        break;
                    }
                    if (tasks.Length == 0)
                    {
                        break;
                    }
                    var timeToSleep = SleepInterval;
                    lock (Output)
                    {
                        if (Output.Length == 0 || !Autoflush)
                        {
                            timeToSleep = Timeout.Infinite;
                        }
                    }

                    var num = Task.WaitAny(tasks, timeToSleep);
                    if (num != -1)
                    {
                        try
                        {
                            if (tasks[num].IsFaulted)
                            {
                                readers[num].Dispose();
                                tasks[num] = null;
                                readers[num] = null;

                                tasks = tasks.Where(x => x != null).ToArray();
                                readers = readers.Where(x => x != null).ToArray();
                            }
                            else if (tasks[num].IsCompleted)
                            {
                                lock (Output)
                                {
                                    Output.AppendLine(tasks[num].Result);
                                }
                                tasks[num] = readers[num].ReadLineAsync();
                            }
                        }
                        catch
                        {

                        }
                    }
                    if (Autoflush && (num == -1 || Output.Length > BufferSize))
                    {
                        Flush(chatId);
                    }
                }
            }
            catch
            {

            }
            if (Autoflush)
            {
                Flush(chatId);
            }
        }
        public void Flush(long chatId)
        {
            lock (Output)
            {
                if (Output.Length == 0)
                {
                    return;
                }
                try
                {
                    foreach (var msg in Ext.SplitBigMessage(Output.ToString()))
                    {
                        TelegramApi.SendTextMessageAsync(chatId, msg).Wait();
                    }
                }
                catch
                {

                }
                Output.Clear();
            }
        }
        
        protected override void ProcessComplete(long chatId)
        {
            InitProcess(chatId);
        }
        public virtual void InitProcess(long chatId)
        {
            if (ExecProcess != null)
            {
                return;
            }
            try
            {
                ExecProcess = new Process();
                ExecProcess.StartInfo = new ProcessStartInfo();
                ExecProcess.StartInfo.WorkingDirectory = Shell.CurrentDirectory;
                ExecProcess.StartInfo.UseShellExecute = false;
                ExecProcess.StartInfo.RedirectStandardInput = true;
                ExecProcess.StartInfo.RedirectStandardOutput = true;
                ExecProcess.StartInfo.RedirectStandardError = true;
                ExecProcess.StartInfo.FileName = Path;
                ExecProcess.StartInfo.Arguments = Arguments;

                ExecProcess.Start();

                ExecProcess.StandardInput.AutoFlush = true;
                Task.Run(() => BeginProxy(chatId, ExecProcess.StandardOutput, ExecProcess.StandardError));
            }
            catch (Exception ex)
            {
                Session.Exit(this);
                TelegramApi.SendTextMessageAsync(chatId, "Exec error: " + ex.Message);
            }
        }
    }
}
