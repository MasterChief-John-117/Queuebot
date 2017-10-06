using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using QueueBot.Entities;

namespace QueueBot
{
    class QueueBot
    {
        public static DiscordShardedClient Client;
        public static Dictionary<ulong, Queue> Queues;
        public static Configuration Config = new Configuration();
        public static Logger Logger = new Logger();

        static void Main(string[] args)
        {
            Config = Config.GetConfiguration();
            Start().GetAwaiter().GetResult();
        }

        public static async Task Start()
        {
            Client = new DiscordShardedClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true
            });

            Client.Log += Logger.LogClientMessage;

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();,

            var serviceProvider = ConfigureServices();

            var _handler = new CommandHandler();
            await _handler.Install(serviceProvider);

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    ThrowOnError = false,
                    DefaultRunMode = RunMode.Async
                }));
            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            return provider;
        }

        public Queue GetQueue(ulong guildId)
        {
            if (Queues.Count < 1)
            {
                Queues = new Dictionary<ulong, Queue> {{guildId, new Queue()}};
                return Queues[guildId];
            }
            else if (!Queues.ContainsKey(guildId))
            {
                Queues.Add(guildId, new Queue());
                return Queues[guildId];
            }
            else
            {
                return Queues[guildId];
            }
        }
    }
}
