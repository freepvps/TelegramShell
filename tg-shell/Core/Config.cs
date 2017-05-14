using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Parser;
using IniParser.Model;
using System.IO;

namespace TgShell.Core
{
    public class Config
    {
        public string ApiKey { get; private set; }
        public string AuthType { get; private set; }

        public static Config Load(string path)
        {
            return Load(new IniDataParser().Parse(File.ReadAllText(path, Encoding.UTF8)));
        }
        public static Config Load(IniData iniData)
        {
            Config cfg = new Config();

            cfg.ApiKey = iniData.GetKey("main.apiKey");
            cfg.AuthType = iniData.GetKey("main.auth");

            return cfg;
        }
    }
}
