using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace QueueBot
{
    public class CommandHandler
    {
        private DiscordShardedClient _client;
        private IServiceProvider _map;
        private CommandService _commands;

        public async Task Install(IServiceProvider map)
        {
            _map = map;
            // Create Command Service, inject it into Dependency Map
            _client = map.GetService(typeof(DiscordShardedClient)) as DiscordShardedClient;
            _commands = new CommandService();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            _client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            // Mark where the prefix ends and the command begins
            int argPos = 0;

            if (QueueBot.Config.UserBlacklist.Contains(parameterMessage.Author.Id)) return;

            if (!message.HasStringPrefix(QueueBot.Config.Prefix, ref argPos)) return;

            var result = _commands.ExecuteAsync(new CommandContext(_client, parameterMessage as IUserMessage), argPos).ConfigureAwait(true).GetAwaiter().GetResult();

            if (!result.IsSuccess)
            {
                if (!result.ErrorReason.Contains("Unknown command"))
                {
                    await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
                }
            }
        }

    }
}
