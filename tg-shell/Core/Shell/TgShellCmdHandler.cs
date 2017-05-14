using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgShell.Core.Shell
{
    public class TgShellCmdHandler : TgSessionCmdHandler
    {
        public virtual TgShellSession Shell { get; set; }
    }
}
