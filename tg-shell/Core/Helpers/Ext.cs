using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgShell.Core.Helpers
{
    public static class Ext
    {
        public static IEnumerable<string> SplitBigMessage(string s, int sizeLimit = 2047)
        {
            if (s.Length < sizeLimit)
            {
                yield return s;
            }
            else
            {
                var lines = s.Split('\n').Select(x => x == null ? string.Empty : x);
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    if (sb.Length + line.Length + 2 < sizeLimit)
                    {
                        sb.AppendLine(line);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                            sb.Clear();
                        }
                        if (line.Length > sizeLimit)
                        {
                            for (var j = 0; j < line.Length; j += sizeLimit)
                            {
                                var length = Math.Min(line.Length - j, sizeLimit);
                                yield return line.Substring(j, length);
                            }
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                }
                yield return sb.ToString();
            }
        }
        static string[,] replaces = new string[,]
        {
            { "»", ">>" },
            { "«", "<<" },
            { "—", "--" },
        };
        public static string TgNormalizeStr(string s)
        {
            for (var i = 0; i < replaces.GetLength(0); i++)
            {
                s = s.Replace(replaces[i, 0], replaces[i, 1]);
            }
            return s;
        }
        public static string ExpandCmd(string s, string cmd, char ch = '/')
        {
            if (s == cmd)
            {
                return s;
            }
            if (!s.EndsWith(cmd))
            {
                return s;
            }
            var to = s.Length - cmd.Length;
            for (var i = 0; i < to; i++)
            {
                if (s[i] != ch)
                {
                    return s;
                }
            }
            return s.Substring(1);
        }
        public static int GetEndOffset(string s, int num = 0, int offset = 0)
        {
            if (s == null) return -1;
            for (var i = offset; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    continue;
                }
                if (s[i] == '\"')
                {
                    int l = i + 1;
                    int r = s.Length;
                    bool skip = false;
                    for (var j = l + 1; j < s.Length; j++)
                    {
                        if (skip)
                        {
                            skip = false;
                            continue;
                        }
                        if (s[j] == '\\')
                        {
                            skip = true;
                        }
                        if (s[j] == '\"')
                        {
                            r = j;
                            break;
                        }
                    }
                    i = r;

                    if (num-- == 0)
                    {
                        return r + 1;
                    }
                }
                else
                {
                    int l = i;
                    int r = s.Length - 1;
                    for (var j = l + 1; j < s.Length; j++)
                    {
                        if (char.IsWhiteSpace(s[j]))
                        {
                            r = j - 1;
                            break;
                        }
                    }
                    if (num-- == 0)
                    {
                        return r + 1;
                    }
                    i = r;
                }
            }
            return -1;
        }
        public static string GetArg(string s, int num = 0)
        {
            if (s == null) return null;
            for (var i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    continue;
                }
                if (s[i] == '\"')
                {
                    int l = i + 1;
                    int r = s.Length;
                    bool skip = false;
                    for (var j = l + 1; j < s.Length; j++)
                    {
                        if (skip)
                        {
                            skip = false;
                            continue;
                        }
                        if (s[j] == '\\')
                        {
                            skip = true;
                        }
                        if (s[j] == '\"')
                        {
                            r = j;
                            break;
                        }
                    }
                    i = r;

                    if (num-- == 0)
                    {
                        return DecodeStr(s.Substring(l + 1, r - l - 1));
                    }
                }
                else
                {
                    int l = i;
                    int r = s.Length - 1;
                    for (var j = l + 1; j < s.Length; j++)
                    {
                        if (char.IsWhiteSpace(s[j]))
                        {
                            r = j - 1;
                            break;
                        }
                    }
                    if (num-- == 0)
                    {
                        return s.Substring(l, r - l + 1);
                    }
                    i = r;
                }
            }
            return null;
        }
        private static string[] emptyStrings = { };
        public static string[] ParseArgs(string s)
        {
            if (s == null) return emptyStrings;
            var result = new List<string>();

            for (var i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i]))
                {
                    continue;
                }
                if (s[i] == '\"')
                {
                    int l = i + 1;
                    int r = s.Length;
                    bool skip = false;
                    for (var j = l + 1; j < s.Length; j++)
                    {
                        if (skip)
                        {
                            skip = false;
                            continue;
                        }
                        if (s[j] == '\\')
                        {
                            skip = true;
                        }
                        if (s[j] == '\"')
                        {
                            r = j;
                            break;
                        }
                    }
                    i = r;

                    result.Add(DecodeStr(s.Substring(l + 1, r - l - 1)));
                }
                else
                {
                    int l = i;
                    int r = s.Length - 1;
                    for (var j = l + 1; j < s.Length; j++)
                    {
                        if (char.IsWhiteSpace(s[j]))
                        {
                            r = j - 1;
                            break;
                        }
                    }
                    result.Add(s.Substring(l, r - l + 1));
                    i = r;
                }
            }
            return result.ToArray();
        }
        public static string DecodeStr(string s, char x = '\\')
        {
            var result = new StringBuilder(s.Length);
            var skip = false;
            foreach (var ch in s)
            {
                if (skip)
                {
                    skip = false;
                    switch (ch)
                    {
                        case '0': result.Append('\0'); continue;
                        case 'r': result.Append('\r'); continue;
                        case 'n': result.Append('\n'); continue;
                        case 't': result.Append('\t'); continue;
                        case 'b': result.Append('\b'); continue;
                        default: result.Append(ch); continue;
                    }
                }
                else
                {
                    if (ch == x)
                    {
                        skip = true;
                    }
                    else
                    {
                        result.Append(ch);
                    }
                }
            }
            return result.ToString();
        }
        public static string GetEmoji(int id)
        {
            return char.ConvertFromUtf32(id);
        }
    }
}
