using System.Collections.Generic;

namespace QueueBot.Entities
{
    public class Queue
    {
        public LinkedList<QueueUser> Users;

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
