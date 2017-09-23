using System;
using System.Xml.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace QueueBot
{
    public class Action
    {
        private string type;
        private string location;
        private string user;

        public Action(CommandContext cx, string command)
        {
            type = command;
            location = cx.Guild.Name + " : " + cx.Channel.Name;
            user = (cx.User as SocketUser) + $"({cx.User.Mention})";
        }


        public string JsonAction(Action action)
        {
            string msg =
                $"{{\"embeds\": [{{\"fields\": [{{\"name\": \"{action.type}\",\"value\": \"{action.user}\\n{action.location}\\n{DateTime.Now}\"}}]}}]}}";
            return msg;
        }
    }
}