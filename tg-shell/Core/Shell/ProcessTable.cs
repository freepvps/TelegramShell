using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgShell.Core.Shell
{
    public class ProcessTable
    {
        public TgShellSession Shell { get; private set; }
        public ProcessTable(TgShellSession shell)
        {
            Shell = shell;
        }

        public Dictionary<string, ExecCmd> Table { get; private set; } = new Dictionary<string, ExecCmd>();

        public object LockObject
        {
            get
            {
                return Table;
            }
        }

        public void CleanUp(bool cleanOnlyComplete = true)
        {
            lock (LockObject)
            {
                List<string> keysToRemove = null;
                foreach (var exec in Table)
                {
                    if ((exec.Value.Started && !exec.Value.Working) || (!cleanOnlyComplete && !exec.Value.Started))
                    {
                        if (keysToRemove == null)
                        {
                            keysToRemove = new List<string>();
                        }
                        keysToRemove.Add(exec.Key);
                    }
                }
                if (keysToRemove != null)
                {
                    foreach (var key in keysToRemove)
                    {
                        Exit(key);
                    }
                }
            }
        }
        public ExecCmd Register(string name, string cmd, string arguments = null)
        {
            var exec = new ExecCmd();
            exec.Path = cmd;
            exec.Arguments = arguments;
            lock (LockObject)
            {
                Exit(name);
                Table[name] = exec;
                return exec;
            }
        }

        public ExecCmd Take(string name)
        {
            lock (LockObject)
            {
                ExecCmd exec;
                Table.TryGetValue(name, out exec);
                return exec;
            }
        }

        public bool Exit(string name)
        {
            lock (LockObject)
            {
                var exec = Take(name);
                if (exec != null)
                {
                    exec.Exit();
                    Table.Remove(name);
                    return true;
                }
                return false;
            }
        }
        public bool Attach(string name)
        {
            lock (LockObject)
            {
                var exec = Take(name);
                if (exec != null)
                {
                    exec.Autoflush = true;
                    Shell.StartHandler(exec);
                    return true;
                }
                return false;
            }
        }
        public bool Detach(string name)
        {
            lock (LockObject)
            {
                var exec = Take(name);
                if (exec != null)
                {
                    exec.Detach();
                    return true;
                }
                return false;
            }
        }
    }
}
