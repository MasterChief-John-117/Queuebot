using System;
using System.IO;
using System.Timers;
using Discord;
using Discord.Commands;
using Newtonsoft;

namespace QueueBot

{
    public class userBlacklist
    {
        public static string[] bringIn() //at run
        {
            string[] list =  System.IO.File.ReadAllLines(@"blacklist.txt"); //set list as string array
            foreach (string str in list)
            {
                MyBot.blackused.Clear();    
                MyBot.blackused.Add(str, 0);
            }
            Console.WriteLine(DateTime.Now + " Blacklist introduced with " + list.Length + " users"); //send out number of users in queue


            return System.IO.File.ReadAllLines(@"blacklist.txt");
        }

        public static void sendOut(Object source, ElapsedEventArgs e) //export blacklist every n seconds
        {
            string[] list = MyBot.blacklist.ToArray(); //convert to array of strings
            System.IO.File.WriteAllLines(@"blacklist.txt", list); //export to text
            //Console.WriteLine(DateTime.Now + " Blacklist updated with " + MyBot.blacklist.Count + " users");
        }


        public static async void commandUsed(CommandEventArgs e)
        {
            if (!MyBot.blackused.ContainsKey(e.Message.User.Id.ToString())) MyBot.blackused.Add(e.Message.User.Id.ToString(), 0);
            //if Dictionary does not have their userID, add it and set their value to zero
            MyBot.blackused[e.Message.User.Id.ToString()]++; //increment how many times they've used it
            e.Message.Delete();
            if (MyBot.blackused[e.Message.User.Id.ToString()] < 3)
            {
                await e.Message.User.SendMessage("You've been blacklisted! You've tried `" +
                                                 MyBot.blackused[e.Message.User.Id.ToString()] +
                                                 "` times. If you try `3` times, mods will be alterted");
            }
            if (MyBot.blackused[e.Message.User.Id.ToString()] == 3)
            {
                await e.Message.User.SendMessage(
                    "You have attemted to use a command `3` times. As such, moderators on this server will be alterted to bot abuse");
                Console.WriteLine(e.Message.User.Name +
                                  " has used a command 3 times post blacklist on server " +
                                  e.Message.Server.Name);
            }
        }
    }
}