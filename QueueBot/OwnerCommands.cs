using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace QueueBot
{

    // Create a module with no prefix
    public class OwnerCommands : ModuleBase
    {
        private Config _config = new Config();

        // generic say hello -> hello
        [Command("say"), Summary("Echos a message."),]
        [RequireOwner]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            await Context.Message.DeleteAsync();
            // ReplyAsync is a method on ModuleBase
            await ReplyAsync(echo);
            Console.WriteLine($"{Context.Message.Author.Username} said {echo}");
        }
        [Command("info")]
        [RequireOwner]
        public async Task info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var embed = new EmbedBuilder().WithTitle("Bot Info")
                .WithDescription(
                    $" - Author: { application.Owner.Username} (ID { application.Owner.Id})\n" +
                    $"- Guilds: {((DiscordSocketClient) Context.Client).Guilds.Count}\n" +
                    $"- Channels: {((DiscordSocketClient) Context.Client).Guilds.Sum(g => g.Channels.Count)}\n" +
                    $"- Users: {((DiscordSocketClient) Context.Client).Guilds.Sum(g => g.Users.Count)}\n\n" +
                    $"{Format.Bold("Process info")}\n" +
                    $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                    $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                    $"- Heap Size: {GetHeapSize()} MB\n" +
                    $"- Uptime: {GetUptime()}\n" +
                    $" - Run {Program.Runtimes} times")
                .WithColor(new Color(114, 137, 218))
                .Build();
            await ReplyAsync("", embed: embed);
        }
        [Command("game")]
        [Alias("setgame")]
        [RequireOwner]
        public async Task SetGame([Remainder, Summary("Text to set the game to")] string game)
        {
            await ((DiscordSocketClient) Context.Client).SetGameAsync(game);
            await Context.Message.AddReactionAsync("✅");
        }
        [Command("restart")]
        [RequireOwner]
        public async Task Restart()
        {
            await Context.Message.AddReactionAsync("👋");
            System.Diagnostics.Process.Start("CMD.exe", "dotnet QueueBot.dll");
            System.Environment.Exit(1);
            await Context.Client.StopAsync();
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}