using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace QueueBot
{
    public class CommandHandler
    {
        public static CommandService _commands;
        private DiscordSocketClient _client;
        private IDependencyMap _map;
        private readonly Config _config = new Config();
        private Utilities utils = new Utilities();
        public LinkedList<ulong> UserBlacklist= JsonConvert.DeserializeObject<LinkedList<ulong>>(File.ReadAllText("blacklist.json"));

        public async Task Install(IDependencyMap map)
        {
            // Create Command Service, inject it into Dependency Map
            _client = map.Get<DiscordSocketClient>();
            _commands = new CommandService();
            //_map.Add(commands);
            _map = map;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var cmd = new CommandHandler();
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;
            // Prevent blacklisted users from issuing commands
            if (cmd.UserBlacklist.Contains(parameterMessage.Author.Id)) return;
            // Make sure the bot can't trigger a command
            if (parameterMessage.Author == _client.CurrentUser) return;
            // Mark where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message has a valid prefix, adjust argPos
            if (parameterMessage.Channel is SocketDMChannel &&
                 parameterMessage.Author.Id != _client.GetApplicationInfoAsync().Result.Owner.Id)
            {
                var owner = _client.GetApplicationInfoAsync().Result.Owner;
                var em = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithName(parameterMessage.Author.Username)
                        .WithIconUrl(parameterMessage.Author.GetAvatarUrl()))
                    .WithDescription(parameterMessage.Content)
                    .WithColor(new Color(128, 119, 218))
                    .WithFooter(new EmbedFooterBuilder().WithText(parameterMessage.Author.Id.ToString()))
                    .WithCurrentTimestamp()
                    .Build();
                await (owner as IUser).CreateDMChannelAsync().Result.SendMessageAsync("", embed: em);
            }
            if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(_config.Prefix(), ref argPos))) return;

            // Create a Command Context
            var context = new CommandContext(_client, message);
            // Execute the Command, store the result
            var result = await _commands.ExecuteAsync(context, argPos, _map);
            int endpos = 0;
            if (message.Content.IndexOf(' ') > argPos) endpos = message.Content.IndexOf(' ');
            Action action = new Action(context, message.Content/*.Substring(argPos, endpos)*/);

            utils.post(action.JsonAction(action));

            // If the command failed, notify the user
            if (!result.IsSuccess)
            	{
	                if (!result.ErrorReason.Contains("suppress"))
	                {
	                    await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
	                }
	            }
            }
        }
    }
