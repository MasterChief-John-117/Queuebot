using System.Net;

namespace QueueBot
{
    public class Token
    {
        public static string[] fromtoken = System.IO.File.ReadAllLines(@"token.txt");
        public static string token = fromtoken[0];
        //public static string token = "MjgwOTQwNzY4NzgzMzY4MTk0.C4Qt_g.Wc1bZu5jS14MfWPvbLNOrzbuKnc";

    }

    public class Ids
    {
        public static string ownerId = "169918990313848832";
        public static string[] whitelist = {ownerId };
        public static string[] contributors = System.IO.File.ReadAllLines(@"contributors.txt");
    }

    public class allcomms
    {
        public static string[] allcoms= {  "qme" + "add" + "queue" + "q" + "up" + "next" + "leave" + "dqme" };
    }
}


