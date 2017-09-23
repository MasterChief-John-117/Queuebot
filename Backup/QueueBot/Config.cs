using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueBot
{
    class Config
    {
        public string Token()
        {
            return File.ReadAllLines("config.txt")[0].Substring(6).Trim().Trim('"').Trim();
        }
        public string Prefix()
        {
            return File.ReadAllLines("config.txt")[1].Substring(7).TrimStart();
        }
    }
}
