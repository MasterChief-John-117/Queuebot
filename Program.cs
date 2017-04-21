﻿using System;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.WebSocket;
using Discord.Commands;



namespace QueueBot
{
    public class Program
    {
        public static string prefix = "=";
        public static int runtimes = Convert.ToInt32(File.ReadAllText("runtimes.txt")) + 1;

        // Convert our sync main to an async main.
        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandHandler handler;
        private Config config;

        public async Task Start()
        {
            File.WriteAllText("runtimes.txt", runtimes.ToString());
            // Define the DiscordSocketClient
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            client.Log += Log;
            config = new Config();

            var token = config.Token();
            Console.WriteLine("Current Prefix: \"" + config.prefix() + "\"");

            // Login and connect to Discord.
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            var map = new DependencyMap();
            map.Add(client);

            handler = new CommandHandler();
            await handler.Install(map);

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