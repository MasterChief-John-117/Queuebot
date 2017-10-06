using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace QueueBot.Entities
{
    public class Configuration
    {
        public string Token;
        public string Prefix;
        public List<ulong> UserBlacklist;
        public List<ulong> GuildBlacklist;

        public Configuration()
        {

        }

        public Configuration(string token, string prefix)
        {
            this.Token = token;
            this.Prefix = prefix;
            this.GuildBlacklist = new List<ulong>();
            this.UserBlacklist = new List<ulong>();
        }

        public void CreateBlankConfiguration()
        {
            File.WriteAllText("files/globalConfiguration.json",
                JsonConvert.SerializeObject(new Configuration(), Formatting.Indented));
        }

        public Configuration GetConfiguration()
        {
            tConfiguration configuration =
                JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("files/globalConfiguration.json"));
            return configuration;
        }
    }
}
