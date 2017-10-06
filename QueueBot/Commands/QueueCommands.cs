using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using QueueBot.Entities;

namespace QueueBot.Commands
{
    public class QueueCommands : ModuleBase
    {
        [Command("AddMe")]
        [Alias("Me")]
        [Summary("Add yourself to the server's queue")]
        public async Task Command_AddMe()
        {
            try
            {
                var guildQueue = QueueBot.GetQueue(Context.Guild.Id);
                if (!guildQueue.Contains(Context.User.Id))
                {
                    guildQueue.Add(Context.User);
                    await ReplyAsync(
                        $"{Context.User.Mention}, you're number `{guildQueue.GetPosition(Context.User.Id)}` in line!");
               }
                else
                {
                    var username = string.IsNullOrEmpty(Context.Guild.GetUserAsync(Context.User.Id).Result.Nickname) ?
                        Context.User.Username : Context.Guild.GetUserAsync(Context.User.Id).Result.Nickname;
                    await ReplyAsync($"You're already in the queue, {username}!");
                }
            }
            catch (Exception e)
            {
                new ExceptionHandler().HandleCommandError(Context, e);
            }
        }

        [Command("Queue")]
        [Alias("Q")]
        [Summary("Returns the current queue for the server")]
        public async Task Command_Queue()
        {
            try
            {
                var guildQueue = QueueBot.GetQueue(Context.Guild.Id);
                string users = "";
                if (guildQueue.Users.Count == 0)
                    users =
                        $"There's no one in the queue! _\\*(You can add yourself with `{QueueBot.Config.Prefix}me`)*_";
                else
                {
                    int i = 1;
                    foreach (var u in guildQueue.Users)
                    {
                        users += $"{i}. {u.User}\n";
                        i++;
                    }
                }

                var random = new Random();
                var emb = new EmbedBuilder()
                    .WithTitle($"Queue for {Context.Guild.Name}")
                    .WithDescription(users)
                    .WithColor(random.Next(0, 50), random.Next(180, 255), random.Next(0,75))
                    .WithCurrentTimestamp();

                await ReplyAsync("", embed: emb.Build());

            }
            catch (Exception e)
            {
                new ExceptionHandler().HandleCommandError(Context, e);
            }
        }
    }
}
