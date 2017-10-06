using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
    }
}
