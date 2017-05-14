using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgShell.Core
{
    public abstract class TgSessionCmdHandler : TgCmdHandler
    {
        public virtual TgSession Session { get; set; }

        public virtual void OnStart(Message msg)
        {
            Process(msg);
        }
        protected override void OnCancel(Message msg)
        {
            Session.Exit(this);
        }
        public virtual void Initialize()
        {

        }
    }
}
