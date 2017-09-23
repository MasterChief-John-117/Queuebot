using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Text.RegularExpressions;

namespace QueueBot
{
    class Utilities
    {
        public LinkedList<SocketUser> GetUsers(CommandContext e)
        {
            LinkedList<SocketUser> users = new LinkedList<SocketUser>();
            string msg = e.Message.Content;
            foreach (var u in e.Message.MentionedUserIds)
            {
                users.AddFirst((e.Client as DiscordSocketClient).GetUser(u));
            }
            try
            {
                IEnumerable<SocketUser> allusers = (e.Client as DiscordSocketClient).Guilds.SelectMany(x => x.Users);
                    MatchCollection matches = Regex.Matches(msg, "[0-9]{18}");
                foreach (var match in matches)
                {
                    foreach (SocketUser u in allusers)
                        if (u.Id.ToString() == match.ToString() && !users.Contains(u))
                            users.AddFirst(u);
                }
            }
            catch (Exception exception)
            {
            }

            return users;
        }

        public void post(string message)
        {
            using (var client = new HttpClient())
            {
                Console.WriteLine("Request: " + client.PostAsync(new Uri("https://canary.discordapp.com/api/webhooks/306881537817968646/im5R3hGNohVT09MMpg7ApjNJuxka-_RAoXpdY26z-J2xNMa7BSXsk6_3YrjljD7Ww4q3"),
                    new StringContent(message, Encoding.UTF8, "application/json")).Result.ReasonPhrase);
            }
        }

    }
}
