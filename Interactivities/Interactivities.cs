using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace leetbot_night.Interactivities
{
    public class Interactivities : BaseCommandModule
    {
        [Command("runpoll"),
         Description("run simple polls.")]
        public async Task PollExtended(CommandContext ctx,
            [Description("`anon` for anonymous poll or `open` to show your name on poll.")] string mode,
            [Description("how long: [number][s/m/h/d].")] TimeSpan duration,
            [Description("up to 20 emotes.")] params DiscordEmoji[] inputoptions)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            bool isAnon = mode != "open";
            if (ctx.Guild != null && isAnon)
                await ctx.Message.DeleteAsync();
            DiscordMessage botrespond = await ctx.RespondAsync("Name poll (abort/cancel to cancel)");
            InteractivityResult<DiscordMessage> variantsmsg = await interactivity.WaitForMessageAsync(xm =>
                xm.ChannelId.Equals(ctx.Message.ChannelId) &&
                xm.Author.Equals(ctx.Message.Author), TimeSpan.FromMinutes(5));
            ulong caughtmsgid = variantsmsg.Result.Id;          // check so it don't fire up on the same message
            if (variantsmsg.Result != null)
            {
                if (variantsmsg.Result.Content == "abort" ||
                    variantsmsg.Result.Content == "cancel")
                {
                    await botrespond.ModifyAsync("Poll cancelled.");
                    if (ctx.Guild != null)
                        await variantsmsg.Result.DeleteAsync();
                }
                else
                {
                    // getting title
                    string pollTitle = variantsmsg.Result.Content;
                    if (ctx.Guild != null) await variantsmsg.Result.DeleteAsync();
                    // getting options
                    var optionNames = new string[inputoptions.Length];
                    IEnumerable<string> pollOptions = inputoptions.Select(xe => xe.ToString());
                    var polldescription = "";
                    var i = 0;
                    foreach (string option in pollOptions)
                    {
                        await botrespond.ModifyAsync($"Name option {option}");
                        variantsmsg = await interactivity.WaitForMessageAsync(xm =>
                            xm.Id != caughtmsgid &&
                            xm.ChannelId.Equals(ctx.Message.ChannelId) &&
                            xm.Author.Equals(ctx.Message.Author));
                        optionNames[i] = variantsmsg.Result.Content;
                        polldescription += $"{option} {optionNames[i]}\n";
                        i++;
                        caughtmsgid = variantsmsg.Result.Id;
                        if (ctx.Guild != null) await variantsmsg.Result.DeleteAsync();
                    }
                    await botrespond.DeleteAsync();     // cleanup

                    // launching poll
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"Poll\n{pollTitle}",
                        Description = polldescription,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"{(!isAnon ? $"Started by: {ctx.User.Username}#{ctx.User.Discriminator}\n" : "")}" +
                                   $"Vote until {DateTime.Now + duration}.\n" +
                                   "To end poll now send `end poll`."
                        }
                    };
                    DiscordMessage pollmsg = await ctx.RespondAsync(embed: embed);

                    foreach (DiscordEmoji option in inputoptions)
                        await pollmsg.CreateReactionAsync(option);
                    // await stop message
                    InteractivityResult<DiscordMessage> stopmsg = await interactivity.WaitForMessageAsync(xm =>
                        xm.ChannelId.Equals(ctx.Message.ChannelId) &&
                        xm.Author.Equals(ctx.Message.Author) &&
                        xm.Content == "end poll", duration);

                    var fullresult = "";

                    pollmsg = await ctx.Channel.GetMessageAsync(pollmsg.Id);            // update message to get reactions

                    int totalReactions = inputoptions.Sum(option => pollmsg.Reactions.FirstOrDefault(xm => xm.Emoji.Equals(option)).Count - 1);

                    i = 0;
                    foreach (DiscordEmoji option in inputoptions)
                    {
                        int reactCount = pollmsg.Reactions.FirstOrDefault(xm => xm.Emoji.Equals(option)).Count - 1;

                        double percentage = reactCount / (double) totalReactions;
                        fullresult += $"{option} {optionNames[i]} : {reactCount} " +
                                      $"({System.Math.Round(percentage * 100, 2)}%)\n";
                        i++;
                    }

                    if (totalReactions == 0)
                        fullresult = "Nobody voted.";
                    var embedresult = new DiscordEmbedBuilder
                    {
                        Title = $"Poll ended\n{pollTitle}",
                        Description = fullresult,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Total votes: {totalReactions}\n" +
                                   $"{(!isAnon ? $"Started by: {ctx.User.Username}#{ctx.User.Discriminator}\n" : "")}" +
                                   $"Ended: {DateTime.Now}"
                        }
                    };
                    await ctx.RespondAsync(embed: embedresult);
                    if (stopmsg.Result != null)
                        if (ctx.Guild != null && isAnon)
                            await stopmsg.Result.DeleteAsync();
                }
            }
            else
                await botrespond.ModifyAsync("Timed out.");
        }

        [Command("waitforcode"), Hidden,
         Description("waits for a response containing a generated code.")]
        public async Task WaitForCode(CommandContext ctx)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            var codebytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(codebytes);
            string code = BitConverter.ToString(codebytes).ToLower().Replace("-", "");
            await ctx.RespondAsync($"Type this code asap: {code}");
            InteractivityResult<DiscordMessage> msg = await interactivity.WaitForMessageAsync(xm =>
                !xm.Author.IsCurrent &&
                xm.Content.Contains(code),
                TimeSpan.FromSeconds(60));
            if (msg.Result != null)
                await ctx.RespondAsync($"And the winner is: {msg.Result.Author.Mention}");
            else
                await ctx.RespondAsync("Nobody? Really?");
        }

        [Command("waitfortyping"), Hidden,
         Description("waits for a typing indicator.")]
        public async Task WaitForTyping(CommandContext ctx)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            InteractivityResult<TypingStartEventArgs> chn = await interactivity.WaitForTypingAsync(ctx.Channel, TimeSpan.FromSeconds(60));
            if (chn.Result != null)
                await ctx.RespondAsync($"{chn.Result.User.Mention}, you typed in " +
                                       $"{chn.Result.Channel.Mention}!");
            else
                await ctx.RespondAsync("*yawn*");
        }
    }
}
