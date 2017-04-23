using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace QueueBot
{
    public class UserCommands : ModuleBase
    {
        Queues _q = Program.qs;
        Config config = new Config();
        private CommandService cmd = CommandHandler._commands;
        private string[] bugs = File.ReadAllLines("bugreports.txt");

        [Command("prefix")]
        [Summary("Shows the prefix if you've forgotten *(meant to be used with an @-mention)*")][Remarks("public")]
        public async Task Prefix()
        {
            await ReplyAsync($"{Context.User.Mention}, try using {Format.Code(config.Prefix())}");
        }
        [Command("me")]
        [Summary("Adds you to the Queue")][Remarks("public")]
        [Alias("qme")]
        public async Task QueueMe()
        {
            bool added = _q.AddUser(_q, Context);

            string msg;
            if (added) msg = "has been added to the queue!";
            else msg = "was already in the queue!";

            var guildUser = Context.User as IGuildUser;
            if (guildUser.Nickname != null)
            {
                await ReplyAsync($"{guildUser.Nickname} {msg}");
            }
            else
            {
                await ReplyAsync($"{guildUser.Username} {msg}");
            }
        }

        [Command("queue")]
        [Summary("Shows the whole queue")][Remarks("public")]
        [Alias("q")]
        public async Task Queue()
        {
            Random rand = new Random();
            int r = rand.Next(0, 255);
            int g = rand.Next(0, 255);
            int b = rand.Next(0, 255);
            var embed = new EmbedBuilder()
                .WithTitle($"Queue for {Context.Guild.Name}")
                .WithDescription(_q.Show(_q, Context))
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithColor(new Color(r, g, b))
                .Build();
            await ReplyAsync("", embed: embed);
        }

        [Command("next")]
        [Summary("Advances the Queue and pings the person who's up")][Remarks("public")]
        [Alias("up")]
        public async Task Next()
        {
            string msg = "Next up is: " + _q.Next(_q, Context);
            if (_q.ServerQueues[Context.Guild].Any())
            {
                _q.ServerQueues[Context.Guild].RemoveFirst();
            }
            if (_q.ServerQueues[Context.Guild].Any())
            {
                var q = _q.ServerQueues[Context.Guild];
                msg += $"\nOn deck, {q.First().Username}#{q.First().Discriminator}";
            }
            await ReplyAsync(msg);

        }


        [Command("help")]
        [Summary("this")]
        public async Task Help()
        {
            string delim = "public";
            if ((Context.User as IGuildUser).GuildPermissions.ManageMessages) delim = "moderator";
            string msg = "";
            if (delim == "moderator")
            {
                msg += Format.Bold("Mod Commands\n");
                foreach (var c in cmd.Commands.Where(c => c.Remarks == "moderator"))
                {
                    msg += c.Name + ": " + c.Summary + "\n";
                }
                msg += Format.Bold("Public Commands\n");
            }
            foreach (var c in cmd.Commands.Where(c => c.Remarks == "public"))
            {
                msg += c.Name + ": " + c.Summary + "\n";
            }
            var embed = new EmbedBuilder().WithTitle("Help!")
                .WithDescription(msg)
                .WithColor(new Color(128, 119, 218))
                .Build();
            await ReplyAsync(Context.User.Mention + ", you needed help?", embed: embed);
        }

        [Command("changelog")]
        [Summary("Shows the changelog for the bot")]
        [Remarks("public")]
        public async Task Changelog()
        {
            string log = File.ReadAllText("changelog.txt");
            var embed = new EmbedBuilder().WithTitle("Recent changes")
                .WithDescription(log)
                .WithColor(new Color(114, 137, 218))
                .Build();
            await ReplyAsync("", embed: embed);
        }

        [Command("reportbug")]
        [Summary("Allows user to send a report to bot owner")]
        [Remarks("public")]
        public async Task ReportBug([Remainder, Summary("Bug to be reported")] string bug)
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var owner = application.Owner;
            var em = new EmbedBuilder().WithTitle($"Bug report from {Context.User.Username}")
                .WithDescription(bug)
                .WithThumbnailUrl(Context.User.GetAvatarUrl())
                .WithColor(new Color(255, 0, 0))
                .WithCurrentTimestamp();
            await owner.CreateDMChannelAsync().Result.SendMessageAsync("", embed: em);
            LinkedList<string> allbugs = new LinkedList<string>(bugs.ToAsyncEnumerable() as IEnumerable<string>);
            allbugs.AddLast(bug);
            await ReplyAsync(":thumbsup: Your bug report has been recieved and will be looked at");
            File.WriteAllLines("bugreports.txt", allbugs);
        }
    }
}
