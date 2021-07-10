using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using leetbot_night.Services;

namespace leetbot_night.Commands
{
    [Group("cat"), Aliases("cats", "kitty", "kitties"),
     Description("A lot of cats here (2000+ pics).")]
    public class Cats : BaseCommandModule
    {
        [Command("cat"), GroupCommand,
         Description("gives you random cat from bot owner collection.")]
        public async Task RandomCat(CommandContext ctx)
        {
            string rndfile = FileHandler.GetRandomFile("D:\\source\\cats", ".png", ".jpg", ".jpeg", ".gif");
            if (rndfile == "File not found.") await ctx.RespondAsync(rndfile);
            else {
                FileStream opened = File.OpenRead(rndfile);
                await new DiscordMessageBuilder().WithFile(opened).SendAsync(ctx.Channel);
                opened.Close();
            }
        }

        [Command("count"),
         Description("gets number of cat pics in bot owner collection.")]
        public async Task CatsCount(CommandContext ctx)
        {
            int catcount = FileHandler.GetFilesCount("D:\\source\\cats", ".png", ".jpg", ".jpeg", ".gif");
            await ctx.RespondAsync($":cat: I've got {catcount} cat pics.");
        }

        [Command("spam"), Aliases("catspam"),
         Description("periodically gives you random cat from bot owner collection.\n" +
                     "Enter `stop cats` to end it (any user can stop it).")]
        public async Task RandomCatSpam(CommandContext ctx,
            [Description("period of cat posting. [number][d/h/m/s]")] TimeSpan period)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            await ctx.RespondAsync(":cat: Cats will appear here every" +
                $"{(period.Days != 0 ? $" {period.Days} day(s)" : "")}" +
                $"{(period.Hours != 0 ? $" {period.Hours} hour(s)" : "")}" +
                $"{(period.Minutes != 0 ? $" {period.Minutes} minute(s)" : "")}" +
                $"{(period.Seconds != 0 ? $" {period.Seconds} second(s)" : "")}.\n" +
                "Enter `stop cats` to stop it.");
            while (true)
            {
                await RandomCat(ctx);
                InteractivityResult<DiscordMessage> stopmsg = await interactivity.WaitForMessageAsync(xm =>
                    xm.ChannelId.Equals(ctx.Message.ChannelId) &&
                    xm.Content == "stop cats", period);
                if (stopmsg.TimedOut) continue;
                await ctx.RespondAsync("Cat spam ends here.");
                break;
            }
        }

        [Command("add"), Hidden, RequireOwner,
         Description("downloads cats.")]
        public async Task CatsAdd(CommandContext ctx)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            DiscordMessage beginmsg = await ctx.RespondAsync(":cat: Send cats now.\n" +
                                                             "Enter `that's it` when you finished.");
            var path = "D:\\source\\cats\\";
            int catcount = FileHandler.GetFilesCount("D:\\source\\cats", ".png", ".jpg", ".jpeg", ".gif");
            ulong caughtmsgid = 0;              // check so it don't fire up on the same message
            while (true)
            {
                InteractivityResult<DiscordMessage> msg = await interactivity.WaitForMessageAsync(xm =>
                    xm.Id != caughtmsgid &&
                    xm.Author == ctx.Message.Author &&
                    (xm.Attachments != null || xm.Content == "that's it"));
                if (msg.TimedOut || msg.Result.Content == "that's it")
                {
                    int newcatcount = FileHandler.GetFilesCount("D:\\source\\cats", ".png", ".jpg", ".jpeg", ".gif");
                    await ctx.RespondAsync($"We're finished. Added {newcatcount - catcount} cat pics.");
                    if (ctx.Guild != null)
                        await ctx.Message.DeleteAsync();    // cleanup
                    await beginmsg.DeleteAsync();
                    if (msg.Result != null && msg.Result.Content == "that's it")
                        if (ctx.Guild != null)
                            await msg.Result.DeleteAsync();
                    break;
                }
                if (msg.Result.Attachments != null)
                {
                    IEnumerable<string> msgattachments = msg.Result.Attachments.Select(xatc => (xatc.Url));
                    foreach (string a in msgattachments)
                        await FileHandler.DownloadUrlFile(a, path, ctx.Client);
                }
                caughtmsgid = msg.Result.Id;
            }
        }
    }
}
