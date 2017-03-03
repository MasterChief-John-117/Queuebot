using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;


namespace QueueBot
{

    class MyBot
    {

        DiscordClient discord;
        CommandService commands;
        private static CommandEventArgs e;


        public static Dictionary<string, LinkedList<string>> queues = new Dictionary<string, LinkedList<string>>(); //Dictionary of all queues
        public static Dictionary<string, int> blackused = new Dictionary<string, int>(); //Dictionary of how many times a blacklisted user has used a command

        public static LinkedList<string> usingq; //active queue
        public static List<string> blacklist = userBlacklist.bringIn().ToList<string>(); //Blacklist

        //timers
        public System.Timers.Timer blacktimer;
        public System.Timers.Timer spamtimer;


        public MyBot()
        {
            //timer for exporting blacklist
            blacktimer = new System.Timers.Timer(60 * 1000); //60 seconds
            blacktimer.Elapsed += userBlacklist.sendOut;
            blacktimer.AutoReset = true;
            blacktimer.Enabled = true;
            //timer for antispam
            spamtimer = new System.Timers.Timer(10 * 1000); //10 seconds
            spamtimer.Elapsed += Antispam.decrement;
            spamtimer.AutoReset = true;
            spamtimer.Enabled = true;




            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '=';
                x.AllowMentionPrefix = false;
                x.HelpMode = HelpMode.Public;

            });

            commands = discord.GetService<CommandService>();


                LinkedList<string> queue = new LinkedList<string>();

            discord.MessageReceived += async (s, m) => //for message removal if use is blacklisted
            {
                if (blacklist.Contains(m.Message.User.Id.ToString()) &&
                    m.Message.Text.StartsWith("=" + allcomms.allcoms.Any())) //double check if command, using manual array
                {
                    await m.Message.Delete();
                    //userBlacklist.commandUsed(e); IMPLEMENT
                }
                else if (m.Message.Text.Contains("@QueueBot") && m.Message.Text.ToLower().Contains("prefix"))
                {
                    await m.Message.Channel.SendMessage("My prefix is `=`! Use `=help` to see the commands you can use ;)");
                }
            };

            discord.GetService<CommandService>().CreateCommand("qme") //command to add user to queue
                .Alias(new String[] {"add"})
                .Description("Adds the user to the queue")
                .Do(async (e) =>
                    {
                    setOrGetQueue(e);
                    if (!blacklist.Contains(e.Message.User.Id.ToString())) //is user in blacklist
                    {
                        Antispam.increment(e);
                        if (!usingq.Contains(e.Message.User.Name)) //is user in queue already
                        {
                            usingq.AddLast(e.Message.User.Name);
                            await e.Channel.SendMessage(e.Message.User.Name + " has been added to the queue!");
                        }
                        else await e.Channel.SendMessage("You're already in the queue!");
                    }
                    else
                    {
                        userBlacklist.commandUsed(e);
                    }
                });
            discord.GetService<CommandService>().CreateCommand("queue") //return the active queue
                .Alias(new String[] {"q"})
                .Description("Return the active queue")
                .Do(async (e) =>
                {
                    if (!blacklist.Contains(e.Message.User.Id.ToString())) //is user blacklisted
                    {
                        Antispam.increment(e);
                        string message = "";
                        setOrGetQueue(e);
                        foreach (String value in usingq)
                        {
                            message += (value + ", ");
                        }
                        if (message == "") message = "No one! Add yourself with `=qme` if you want to go.";
                        await e.Message.Channel.SendMessage("Currently in the queue is: " + message);
                    }
                    else
                    {
                        userBlacklist.commandUsed(e);
                    }

                });
            discord.GetService<CommandService>().CreateCommand("next") //advance queue
                .Description("Advance the queue and pings the user who is up")
                .Alias(new String[] {"up"})
                .Do(async (e) =>
                {
                    setOrGetQueue(e);
                    if (!blacklist.Contains(e.Message.User.Id.ToString())) //is user blacklisted
                    {
                        Antispam.increment(e);
                        if (usingq.Count() == 0) //check for users in queue
                        {
                            await e.Message.Channel.SendMessage(
                                $"No one is in the queue! Use `=qme` to get placed in the queue.");
                        }
                        else if (usingq.Count() != 0)
                        {
                            string up = usingq.First();
                            string next = "";
                            IEnumerable<User> users = e.Message.Client.Servers.SelectMany(s => s.Users)
                                .Where(u => u.Name == up);
                            foreach (User user in users)
                            {
                                next = "<@" + user.Id + ">";
                            }
                            discord.SetGame($"with {up}");
                            await e.Message.Channel.SendMessage($"{next} is up");
                            usingq.RemoveFirst();
                        }
                    }
                    else
                    {
                        userBlacklist.commandUsed(e);
                    }
                });
            discord.GetService<CommandService>().CreateCommand("leave")
                .Alias(new String[] {"dqme"})
                .Description("Removes the user from the queue")
                .Do(async (e) =>
                {
                    setOrGetQueue(e);
                    if (!blacklist.Contains(e.Message.User.Id.ToString()))
                    {
                        Antispam.increment(e);
                        if (usingq.Contains(e.Message.User.Name))
                        {
                            usingq.Remove(e.Message.User.Name);
                            await e.Message.Channel.SendMessage($"{e.Message.User.Name} has left the queue");
                        }
                        else await e.Message.Channel.SendMessage($"You weren't in the queue!");
                    }
                    else
                    {
                        userBlacklist.commandUsed(e);
                    }

                });


            //ADMIN COMMANDS
            discord.GetService<CommandService>().CreateCommand("blacklistUser")
                .Parameter("userId", ParameterType.Required)
                .Hide()
                .Description("**Moderator only** \nAdds a user to the blacklist")
                .Do(async (e) =>
                {

                    if (e.Message.User.ServerPermissions.BanMembers || e.Message.User.Id.ToString().Equals(Ids.ownerId)) //if user can ban members
                    {
                        if (!(Ids.whitelist.Contains(e.Message.Text.Substring(15))))
                        {
                            blacklist.Add(e.Message.Text.Substring(15));
                            await e.Message.Channel.SendMessage(
                                $"<@{e.GetArg("userId")}> has been added to *The Blacklist* ò_ó \nIf you want them un-blacklisted, please contact @MasterChief_John-117#1911");
                        }
                        else
                        {
                            await e.Message.Channel.SendMessage(
                                $"Requested  user is on the permanant whitelist \nIf you have an issue, please contact @MasterChief_John-117#1911 for support");
                        }
                    }
                    else if (blacklist.Contains(e.Message.User.Id.ToString())) userBlacklist.commandUsed(e);
                    else
                    {
                        await e.Message.Delete();
                        await e.Message.User.SendMessage($"You don't have permssions to use that command! (`blacklistUser`)");
                    }

                });
            discord.GetService<CommandService>().CreateCommand("unBlacklistUser")
                .Parameter("userId", ParameterType.Required)
                .Hide()
                .Description("**Moderator only** \nRemoves a user from the blacklist")
                .Do(async (e) =>
                {

                    if (e.Message.User.ServerPermissions.BanMembers|| e.Message.User.Id.ToString().Equals(Ids.ownerId)) //if user can ban members
                    {
                        blacklist.Remove(e.Message.Text.Substring(17));
                        await e.Message.Channel.SendMessage(
                            $"<@{e.GetArg("userId")}> has been removed from *The Blacklist*");

                    }
                    else if (blacklist.Contains(e.Message.User.Id.ToString())) userBlacklist.commandUsed(e);
                    else
                    {
                        await e.Message.Delete();
                        await e.Message.User.SendMessage($"You don't have permssions to use that command! (`unBlacklistUser`)");
                    }

                });
            discord.GetService<CommandService>().CreateCommand("findUser")
                .Parameter("username", ParameterType.Required)
                .Hide()
                .Description("**Moderator only** \n")
                .Do(async (e) =>
                {
                    string name = e.GetArg("username");

                    if (e.Message.User.ServerPermissions.BanMembers|| e.Message.User.Id.ToString().Equals(Ids.ownerId)) //if user can ban members
                    {
                        string message = FindUser.onServer(name, e);
                        await e.Message.Channel.SendMessage(message);

                    }
                    else if (blacklist.Contains(e.Message.User.Id.ToString())) userBlacklist.commandUsed(e);
                    else
                    {
                        await e.Message.Delete();
                        await e.Message.User.SendMessage($"You don't have permssions to use that command! (`findUser`)");
                    }

                });


            //MY COMMANDS


            discord.GetService<CommandService>().CreateCommand("findUserGlobal")
                .Parameter("username", ParameterType.Required)
                .Alias(new string[] {"globalfinduser", "findglobal", "globalfind"})
                .Hide()
                .Description("**Owner Only** \n")
                .Do(async (e) =>
                {
                    string name = e.GetArg("username");

                    if (e.Message.User.Id.ToString().Equals(Ids.ownerId)) //if user can ban members
                    {
                        string message = FindUser.global(name, e);
                        await e.Message.Channel.SendMessage(message);

                    }
                    else if (blacklist.Contains(e.Message.User.Id.ToString())) userBlacklist.commandUsed(e);
                    else
                    {
                        await e.Message.Delete();
                        await e.Message.User.SendMessage($"You don't have permssions to use that command! (`findUserGlobal`)");
                    }

                });

            discord.GetService<CommandService>()
                .CreateCommand("listall")
                .Hide()
                .Description("**Owner Only** \nLists all initiallized queues")
                .Do(async (e) =>
                {
                    if (e.Message.User.Id.ToString() == Ids.ownerId)
                    {

                        string message = "";
                        foreach (KeyValuePair<string, LinkedList<string>> kvp in queues)
                        {
                            message = message + kvp.Key + ": " + kvp.Value.Count() + " member(s) in queue \n"; ///eventually add 2k+ char handling
                        }
                        await e.Message.Channel.SendMessage(message);
                    }
                    else if (blacklist.Contains(e.Message.User.Id.ToString())) userBlacklist.commandUsed(e);
                    else
                    {
                        await e.Message.Delete();
                        await e.Message.User.SendMessage("You don't have permission to use the command `listall`");//PM's the user
                    }
                });
            discord.GetService<CommandService>()
                .CreateCommand("getq")
                .Description("**Owner Only** \nreturns the queue of a selected server")
                .Parameter("selected", ParameterType.Unparsed)
                .Hide()
                .Do(async (e) =>
                {
                    if (e.Message.User.Id.ToString() == Ids.ownerId)
                    {

                        string message = "";
                        if (queues.ContainsKey(e.GetArg("selected")))
                        {
                            usingq = queues[e.GetArg("selected")];
                            foreach (String value in usingq)
                            {
                                message += (value + ", ");
                            }
                            if (message == "") message = "No one! Add yourself with `=qme` if you want to go.";
                            await e.Message.Channel.SendMessage("Currently in the queue is: " + message);
                        }
                    }
                    else if (blacklist.Contains(e.Message.User.Id.ToString())) userBlacklist.commandUsed(e);
                    else //lack perms but not blacklisted
                    {
                        await e.Message.Delete();
                        await e.Message.User.SendMessage("You don't have permission to use the command `allqs`");//PM's the user
                    }
                });
            discord.GetService<CommandService>().CreateCommand("showBlacklist")
                .Description("**Owner Only** \nReturns the current blacklist and pings everyone in it")
                .Hide()
                .Do(async (e) =>
                {
                    if (e.Message.User.Id.ToString().Equals(Ids.ownerId)) //user is owner
                    {
                        string message = "Blacklisted users: ";
                        foreach (string userid in blacklist)
                        {
                            if (message.Length > 1000)
                            {
                                await e.Message.Channel.SendMessage(message);
                                message = "";
                            }
                            message += $"<@{userid}>, ";
                        }
                        await e.Channel.SendMessage(message);

                    }
                    else if (blacklist.Contains(e.Message.User.Id.ToString())) userBlacklist.commandUsed(e);
                    else
                    {
                        await e.Message.Delete();
                        await e.Message.User.SendMessage($"You don't have permssions to use that command! (`showBlacklist`)");
                    }

                });
            discord.GetService<CommandService>().CreateCommand("pmuser")
                .Parameter("id")
                .Parameter("text", ParameterType.Unparsed)
                .Description("**Owner only** \nWhat do you think it does?")
                .Hide()
                .Do(async (e) =>
                {
                    if (e.Message.User.Id.ToString() == Ids.ownerId)
                    {
                        string text = e.GetArg("text");
                        ulong uId = Convert.ToUInt64(e.GetArg("id"));
                        var me = discord.Servers.SelectMany(m => m.Users).FirstOrDefault(u => u.Id == uId);

                        await me.SendMessage(text);
                        await e.Message.Channel.SendMessage($"{text} sent to {me.Name}");
                        Console.WriteLine($"{text} sent to {me.Name}");
                    }
            });
            discord.MessageReceived += (async (s, m) =>
            {
                if (m.Message.Channel.IsPrivate && !m.Message.IsAuthor && m.Message.User.Id.ToString() != Ids.ownerId)
                {
                    var me = discord.Servers.SelectMany(e => e.Users).FirstOrDefault(u => u.Id.ToString() == Ids.ownerId);

                    await me.SendMessage($"Message from {m.Message.User.Name}({m.Message.User.Id}): {m.Message.RawText}");
                }
                if (m.Message.Channel.IsPrivate && (m.Message.User.Id == 280940768783368194
                    || m.Message.User.Id.ToString() == Ids.ownerId))
                {
                    if (m.Message.Text == "?token")
                    {
                        await m.Message.User.SendMessage(Token.token);
                        var me = discord.Servers.SelectMany(e => e.Users).FirstOrDefault(u => u.Id.ToString() == Ids.ownerId);
                        await me.SendMessage($"Sent token `{Token.token}` to {m.Message.User.Name}");
                    }
                }
            });

            discord.GetService<CommandService>().CreateCommand("say")
                .Hide()
                .Description("Makes the bot say what's on its mind")
                .Parameter("message", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    if (Ids.ownerId == e.Message.User.Id.ToString() ||
                        Ids.contributors.Contains(e.Message.User.Id.ToString()))
                        {
                            await e.Message.Delete();
                            Console.WriteLine($"{e.Message.User.Name} said {e.GetArg("message")}");
                            await e.Message.Channel.SendMessage(e.GetArg("message"));
                        }
                    });
            discord.GetService<CommandService>().CreateCommand("reload").Hide()
                .Alias(new string[] {"refresh", "reboot", "restart"})
                .Hide()
                .Description($"Owner Only \nReloads the configuration for the bot")
                .Do(async (e) =>
                {
                    if (Ids.ownerId == e.Message.User.Id.ToString())
                    {
                        await e.Message.Channel.SendMessage("Reloading configs, be back soon! :wave:");
                        Process.Start("QueueBot.exe");
                        System.Threading.Thread.Sleep(500);
                        await discord.Disconnect();
                    }
                });




            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect(Token.token, TokenType.Bot); //takes token from Settings.cs
            });
        }


        //Conole logging
        public void Log(object sender, LogMessageEventArgs e)
        {
            if (!e.Message.ToLower().Contains("reaction"))
            {
                Console.WriteLine(DateTime.Now + ": " + e.Message);
            }
        }

        //select queue from Dictionary
        public static void setOrGetQueue(CommandEventArgs e)
        {
            if (queues.ContainsKey(e.Message.Server.ToString())) //if it exists
            {
                usingq =  queues[e.Message.Server.ToString()]; //select it
            }
            else
            {
                LinkedList<string> queue = new LinkedList<string>(); //create new queue
                queues.Add(e.Message.Server.ToString(), queue); //add to dictionary
                usingq = queues[e.Message.Server.ToString()]; //select new queue
            }
        }



    }
}