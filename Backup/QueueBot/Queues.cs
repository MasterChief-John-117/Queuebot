using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace QueueBot
{
    public class Queues
    {
        public Dictionary<IGuild, LinkedList<IUser>> ServerQueues = new Dictionary<IGuild, LinkedList<IUser>>();
        private Config _config = new Config();

        public void AddServer(Queues thisqueue, IGuild guild)
        {
            thisqueue.ServerQueues.Add(guild, new LinkedList<IUser>());
        }

        public bool AddUser(Queues thisqueue, ICommandContext context)
        {
            if (!thisqueue.ServerQueues.ContainsKey(context.Guild))
            {
                AddServer(thisqueue, context.Guild);
            }
            if (!thisqueue.ServerQueues[context.Guild].Contains(context.User))
            {
                thisqueue.ServerQueues[context.Guild].AddLast(context.User);
                return true;
            }
            else return false;
        }

        public string Show(Queues thisqueue, ICommandContext context)
        {
            if (!thisqueue.ServerQueues.ContainsKey(context.Guild))
            {
                AddServer(thisqueue, context.Guild);
            }
            if (!thisqueue.ServerQueues[context.Guild].Any())
            {
                return $"No one in the queue! Use {_config.Prefix()}qme to be entered into the queue.";
            }
            else
            {
                var q = thisqueue.ServerQueues[context.Guild];
                string msg = "";
                int i = 1;
                foreach (var u in q)
                {
                    var guildUser = u as IGuildUser;
                    if (guildUser.Nickname != null)
                    {
                        msg += ($"{i}. {guildUser.Nickname}#{guildUser.Discriminator}\n");
                    }
                    else
                    {
                        msg += ($"{i}. {guildUser.Username}#{guildUser.Discriminator}\n");
                    }
                    i++;
                }
                return msg;
            }
        }

        public string Next(Queues thisqueue, ICommandContext context)
        {
            if (!thisqueue.ServerQueues.ContainsKey(context.Guild))
            {
                AddServer(thisqueue, context.Guild);
                return ($"The queue for {context.Guild} has no members! Add yourself with {_config.Prefix()}qme.");
            }
            var q = thisqueue.ServerQueues[context.Guild];
            if (q.Any())
            {
                string msg = q.First().Mention;
                //if (q.Count() > 1) msg += $"\nOn deck, {q.First().Username}#{q.First().Discriminator}";
                return msg;
            }
            else return ($"The queue for {context.Guild} has no members! Add yourself with `{_config.Prefix()}qme`.");
        }
    }
}

