using System;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.WebSocket;
using Discord.Commands;



namespace QueueBot
{
    public class Program
    {
        public static int Runtimes = Convert.ToInt32(File.ReadAllText("runtimes.txt")) + 1;
        public static Queues qs = new Queues();


        // Convert our sync main to an async main.
        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandHandler _handler;
        private Config _config;

        public async Task Start()
        {
            File.WriteAllText("runtimes.txt", Runtimes.ToString());
            // Define the DiscordSocketClient
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            _config = new Config();

            var token = _config.Token();
            Console.WriteLine("Current Prefix: \"" + _config.Prefix() + "\"");

            // Login and connect to Discord.
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            var map = new DependencyMap();
            map.Add(_client);

            _handler = new CommandHandler();
            await _handler.Install(map);

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}