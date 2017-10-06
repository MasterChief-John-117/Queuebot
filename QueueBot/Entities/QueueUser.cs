using System.Collections.Generic;
using Discord;

namespace QueueBot.Entities
{
    public class QueueUser
    {
        public IUser User;
        public string Song;
        public Dictionary<ulong, int> GuildReputation;

        public QueueUser(IUser user)
        {
            this.User = user;
            this.Song = "";
        }
        public QueueUser(IUser user, string song)
        {
            this.User = user;
            this.Song = song;
        }

        public QueueUser AddSong(string song)
        {
            this.Song = song;
            return this;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Song))
            {
                return User.ToString();
            }
            else
            {
                return $"{User} - {Song}";
            }
        }
    }
}
