using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;

namespace QueueBot
{
    public class FindUser
    {
        public static Stopwatch stopwatch = new Stopwatch();


        public static string onServer(string name, CommandEventArgs e)
        {
            IEnumerable<User> users = e.Message.Client.Servers.SelectMany(s => s.Users);
            int count = 0;
            string people = "";
            LinkedList<ulong> ids = new LinkedList<ulong>();
            foreach (User user in users)
            {
                if (!String.IsNullOrEmpty(user.Nickname) && user.Nickname.ToLower().Contains(name)
                 && !ids.Contains(user.Id) && user.Server == e.Message.Server)
                {
                    Console.WriteLine(user.Name);
                    count = count + 2;
                    people += user.Name + " (" + user.Nickname + ") : " + user.Id.ToString() + "\n";
                    ids.AddFirst(user.Id);
                }
                if(user.Name.ToLower().Contains(name) && !ids.Contains(user.Id) && user.Server == e.Message.Server)
                {
                    Console.WriteLine(user.Name);
                    count++;
                    people += user.Name + " : " + user.Id.ToString() + "\n";
                    ids.AddFirst(user.Id);
                }
            }
            if (count > 20)
            {
                return $"I found `{count}` users! Please use more strict parameters.. :sweat_smile: ";
            }
            else if (count >= 1 && count <= 20)
            {
                return people;
            }
            else
            {
                return "I couldn't find anyone :(";
            }
        }

        public static string global(string name, CommandEventArgs e)
        {
            stopwatch.Restart();
            IEnumerable<User> users = e.Message.Client.Servers.SelectMany(s => s.Users);
            int count = 0;
            string people = "";
            string rgx = name;
            LinkedList<ulong> ids = new LinkedList<ulong>();
            foreach (User user in users)
            {
                if (!String.IsNullOrEmpty(user.Nickname) && Regex.IsMatch(user.Nickname, rgx, RegexOptions.IgnoreCase))
                {
                    Console.WriteLine(user.Name);
                    count++;
                    people += user.Name + " (" + user.Nickname + ") : " + user.Id.ToString() + "\n";
                    ids.AddFirst(user.Id);
                }
                if(Regex.IsMatch(user.Name.ToLower(), rgx, RegexOptions.IgnoreCase) && !ids.Contains(user.Id))
                {
                    Console.WriteLine(user.Name);
                    count++;
                    people += user.Name + " : " + user.Id.ToString() + "\n";
                    ids.AddFirst(user.Id);
                }
            }
            if (count > 20)
            {
                stopwatch.Stop();
                return $"I found `{count}` users! Please use more strict parameters.. :sweat_smile: (Search took `{stopwatch.ElapsedMilliseconds}`ms)";
            }
            else if (count >= 1 && count <= 20)
            {
                stopwatch.Stop();
                return $"{people}(Search took `{stopwatch.ElapsedMilliseconds}`ms)";
            }
            else
            {
                stopwatch.Stop();
                return $"It took me `{stopwatch.ElapsedMilliseconds}`ms to find no one :(";
            }
        }
    }
}