using System.Collections.Generic;
using System.Linq;
using Discord;

namespace QueueBot.Entities
{
    public class Queue
    {
        public LinkedList<QueueUser> Users;

        public Queue()
        {
            Users = new LinkedList<QueueUser>();
        }
        public Queue Add(QueueUser user)
        {
            Users.AddLast(user);
            return this;
        }

        public Queue Add(ulong userId)
        {
            Users.AddLast(new QueueUser(QueueBot.Client.GetUser(userId)));
            return this;
        }

        public Queue Add(IUser user)
        {
            Users.AddLast(new QueueUser(user));
            return this;
        }

        public bool Contains(ulong userId)
        {
            return this.Users.Select(q => q.User.Id).Contains(userId);
        }

        public int GetPosition(ulong userId)
        {
            return Users.ToList().FindIndex(u => u.User.Id == userId) + 1;
        }

        public override string ToString()
        {
            if (Users.Count == 0) return $"This queue is empty!";
            else
            {
                string str = "";
                int i = 1;
                foreach (var user in Users)
                {
                    str += $"{i}. {user}\n";
                }
                return str;
            }
        }
    }
}
