using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TgShell.Core;

namespace TgShell
{
    class Program
    {
        public static Config Config { get; private set; }
        public static TgShellWorker TgShellWorker { get; private set; }
        static void Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                args = new string[]
                {
                    "tgshell.conf"
                };
            }
            Config = Config.Load(args.First());
            TgShellWorker = new TgShellWorker();

            TgShellWorker.Configure(Config);
            TgShellWorker.Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
