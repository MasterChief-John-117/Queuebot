using System;
using Discord;
using Discord.Commands;

namespace QueueBot.Entities
{
    public class ExceptionHandler
    {
        public async void HandleCommandError(ICommandContext Context, Exception e)
        {
            await QueueBot.Logger.LogErrorMessage(e.ToString());
            await Context.Channel.TriggerTypingAsync();
            await Context.Client.GetApplicationInfoAsync().Result.Owner.SendMessageAsync($"User: `{Context.Message.Author}`\nMessage: `{Context.Message.Content}`\n```\n{e}\n```");
            await Context.Channel.SendMessageAsync(
                $"Whoops! An error occured. A bug report has been sent to the developer");
        }
    }
}
