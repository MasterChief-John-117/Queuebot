using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace QueueBot
{
    public class ModCommands : ModuleBase
    {
        private readonly Config _config = new Config();
        private CommandService cmd = CommandHandler._commands;

        [Command("clean")] //code sourced and modified from NadekoBot
        [Summary("Removes all messages by the bot and messages that are command messages in the last up to 100 messages")][Remarks("moderator")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task Clean(int count = 100)
        {
            if (count < 1)
                return;

            int limit = (count < 100) ? count : 100;
            var enumerable = (await Context.Channel.GetMessagesAsync(limit: limit).Flatten())
                .Where(m => (m.Author.Id == Context.Client.CurrentUser.Id)
                            || m.ToString().Substring(0, _config.Prefix().Length) == _config.Prefix()); //select messages

            LinkedList<IMessage> msgstodel = new LinkedList<IMessage>(enumerable);

            while (DateTime.UtcNow -
                   new DateTime((long) ((msgstodel.Last().Id / 4194304 + 1420070400000) * 10000 +
                                        621355968000000000)) > TimeSpan.FromDays(14))
            {
                msgstodel.RemoveLast();
            }
            await Context.Channel.DeleteMessagesAsync(msgstodel);
        }
        [Command("qclean")]
        [Summary("Removes all messages by the bot in the last up to 100 messages")][Remarks("moderator")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task QClean(int count = 100)
        {
            if (count < 1)
                return;

            int limit = (count < 100) ? count : 100;
            var enumerable = (await Context.Channel.GetMessagesAsync(limit: limit).Flatten())
                .Where(m => (m.Author.Id == Context.Client.CurrentUser.Id));
            LinkedList<IMessage> msgstodel = new LinkedList<IMessage>(enumerable);

            while (DateTime.UtcNow -
                   new DateTime((long) ((msgstodel.Last().Id / 4194304 + 1420070400000) * 10000 +
                                        621355968000000000)) > TimeSpan.FromDays(14))
            {
                msgstodel.RemoveLast();
            }
            await Context.Channel.DeleteMessagesAsync(msgstodel);

            await Context.Message.DeleteAsync();
        }
    }
}