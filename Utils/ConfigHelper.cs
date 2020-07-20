using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stash.Utils
{
    public class Config
    {
        public int width { get; set; }
        public int height { get; set; }
        public bool safeZoneOnly { get; set; }
        public string address { get; set; }
        public string name { get; set; }
        public string table { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int port { get; set; }
    }

    public class ConfigHelper
    {
        public static void EnsureConfig(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("No config.json");

                JObject stashConfig = new JObject();
                stashConfig.Add("width", 6);
                stashConfig.Add("height", 6);
                stashConfig.Add("safeZoneOnly", false);
                stashConfig.Add("address", "127.0.0.1");
                stashConfig.Add("name", "unturned");
                stashConfig.Add("table", "stash");
                stashConfig.Add("username", "root");
                stashConfig.Add("password", "root");
                stashConfig.Add("port", 3306);

                // write JSON directly to a file
                using (StreamWriter file = File.CreateText(path))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    stashConfig.WriteTo(writer);
                    Console.WriteLine("Generated Stash config");
                }
            }
        }

        public static Config ReadConfig(string path)
        {
            using (StreamReader file = File.OpenText(path))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                return JsonConvert.DeserializeObject<Config>(JToken.ReadFrom(reader).ToString());
            }
        }
    }
}
