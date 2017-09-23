using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace QueueBot
{

    // Create a module with no prefix
    public class OwnerCommands : ModuleBase
    {
        private Config _config = new Config();
        private string[] bugs = File.ReadAllLines("bugreports.txt");
        private CommandHandler cmd = new CommandHandler();
        private Utilities utils = new Utilities();

        // generic say hello -> hello
        [Command("say"), Summary("Echos a message.")]
        [Priority(int.MaxValue)]
        [RequireOwner]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            await Context.Message.DeleteAsync();
            if (echo.Length > 18)
            {
                string channel = echo.TrimStart().Substring(0, 18);
                if (Regex.IsMatch(channel, "[0-9]{18}"))
                {
                    var msgchannel = Context.Client.GetChannelAsync(Convert.ToUInt64(channel));
                    await (msgchannel.Result as IMessageChannel).SendMessageAsync(echo.TrimStart().Substring(18));
                }
                else await ReplyAsync(echo);
            }
            // ReplyAsync is a method on ModuleBase
            else await ReplyAsync(echo);
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
                    $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum((x => x.Users.Count()))}\n\n" +
                    $"{Format.Bold("Process info")}\n" +
                    $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                    $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                    $"- Heap Size: {GetHeapSize()} MB\n" +
                    $"- Uptime: {GetUptime()}\n" +
                    $" - Run {Program.Runtimes} times")
                .WithColor(new Color(114, 137, 218))
                .Build();
            await Task.Yield();
            await ReplyAsync("", embed: embed);
        }
        [Command("blacklistuser")]
        [RequireOwner]
        public async Task BlacklistUser([Remainder] string uselessAPI)
        {
            ulong id = utils.GetUsers(Context as CommandContext).FirstOrDefault().Id;
            if (!cmd.UserBlacklist.Contains(id))
            {
                cmd.UserBlacklist.AddFirst(id);
                File.WriteAllText("blacklist.json", JsonConvert.SerializeObject(cmd.UserBlacklist, Formatting.Indented));
            }
            await ReplyAsync($"Sucks to be {(Context.Client as DiscordSocketClient).GetUser(id)}!");
        }
        [Command("unblacklistuser")]
        [RequireOwner]
        public async Task UnBlacklistUser([Remainder] string uselessAPI)
        {
            ulong id = utils.GetUsers(Context as CommandContext).FirstOrDefault().Id;
            if (cmd.UserBlacklist.Contains(id))
            {
                cmd.UserBlacklist.Remove(id);
                File.WriteAllText("blacklist.json", JsonConvert.SerializeObject(cmd.UserBlacklist, Formatting.Indented));
            }
            await ReplyAsync($"Guess someone took pity on you, {(Context.Client as DiscordSocketClient).GetUser(id)}");
        }

        [Command("dmuser")]
        [Alias("pmuser")]
        public async Task PMUser(string user, [Remainder] string uselessAPI)
        {
            var u = utils.GetUsers(Context as CommandContext).FirstOrDefault();
            await(u as SocketUser).CreateDMChannelAsync().Result.SendMessageAsync(uselessAPI);
            await ReplyAsync($"Sent {uselessAPI} to {u.Username}");
        }
        [Command("game")]
        [Alias("setgame")]
        [Priority(1)]
        [RequireOwner]
        public async Task SetGame([Remainder, Summary("Text to set the game to")] string game)
        {
            await ((DiscordSocketClient) Context.Client).SetGameAsync(game);
            await Context.Message.AddReactionAsync("✅");
        }

        [Command("getbugs")]
        [RequireOwner]
        public async Task GetBugs()
        {
            Dictionary<int, string> bugs = new Dictionary<int, string>();
            int i = 1;
            foreach (string s in this.bugs)
            {
                bugs.Add(i, s);
                i++;
            }
            string msg = "";
            foreach (KeyValuePair<int, string> kvp in bugs)
            {
                msg += kvp.Key + ": " + kvp.Value + "\n";
            }

            var em = new EmbedBuilder()
                .WithTitle("Current Open Bugs")
                .WithDescription(msg)
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync("", embed: em);
        }

        [Command("closebug")]
        [RequireOwner]
        public async Task CloseBug(int index)
        {
            Dictionary<int, string> allbugs = new Dictionary<int, string>();
            int i = 1;
            foreach (string s in bugs)
            {
                allbugs.Add(i, s);
                i++;
            }
            if (index >= i)
            {
                await ReplyAsync("The requested bug does not exist");
                return;
            }
            string closedbug = allbugs[index];
            allbugs.Remove(index);
            string msg = "";
            foreach (KeyValuePair<int, string> kvp in allbugs)
            {
                msg +=  kvp.Value + "\n";
            }
            File.WriteAllText("bugreports.txt", msg);
            await ReplyAsync($"The bug `{closedbug}` has been closed");

        }
        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}