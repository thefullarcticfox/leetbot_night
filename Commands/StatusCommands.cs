using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace leetbot_night.Commands
{
    public class StatusCommands : BaseCommandModule
    {
        [Command("status"),
         Description("gets user status.")]
        public async Task Status(CommandContext ctx,
            [Description("user to get status from. (default: your status)")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;
            var status = "**User status:** ";
            var color = new DiscordColor(0x747F8D);
            if (user.Presence == null)
                status += "Offline";
            else
            {
                color = user.Presence.Status switch
                {
                    UserStatus.Online => new DiscordColor(0x43B581),
                    UserStatus.Idle => new DiscordColor(0xFAA61A),
                    UserStatus.DoNotDisturb => new DiscordColor(0xF04747),
                    _ => new DiscordColor(0x747F8D)
                };
                status += $"{user.Presence.Status} " +
                          $"({(user.Presence.ClientStatus.Desktop.HasValue ? "Desktop" : user.Presence.ClientStatus.Web.HasValue ? "Web" : user.Presence.ClientStatus.Mobile.HasValue ? "Mobile" : "Bot")})\n";
                foreach (DiscordActivity activity in user.Presence.Activities)
                {
                    if (activity.ActivityType.Equals(ActivityType.Custom))
                    {
                        status += "**Custom status:** ";
                        if (activity.CustomStatus.Emoji != null)
                            status += activity.CustomStatus.Emoji.GetDiscordName() + " ";
                        if (activity.CustomStatus.Name != null)
                            status += activity.CustomStatus.Name;
                        status += "\n";
                    }
                    if (activity.ActivityType.Equals(ActivityType.ListeningTo))
                    {
                        status += "\n:headphones: **Listening to** ";
                        if (activity.RichPresence.Details != null)
                            status += "\n" + activity.RichPresence.Details;
                        else
                            status += "Untitled";
                        if (activity.RichPresence.State != null)
                            status += "\nby " + activity.RichPresence.State;
                        if (activity.RichPresence.LargeImageText != null)
                            status += "\non " + activity.RichPresence.LargeImageText;
                        status += "\n";
                    }
                    if (activity.ActivityType.Equals(ActivityType.Playing))
                    {
                        status += "\n:video_game: **Playing** ";
                        status += activity.Name;
                        if (activity.RichPresence != null)
                        {
                            if (activity.RichPresence.State != null)
                                status += "\nState: " + activity.RichPresence.State;
                            if (activity.RichPresence.Details != null)
                                status += "\nDetails: " + activity.RichPresence.Details;
                            if (activity.RichPresence.LargeImageText != null)
                                status += "\nLimgTxt: " + activity.RichPresence.LargeImageText;
                            if (activity.RichPresence.SmallImageText != null)
                                status += "\nSimgTxt: " + activity.RichPresence.SmallImageText;
                        }
                        status += "\n";
                    }
                    if (activity.ActivityType.Equals(ActivityType.Watching))
                    {
                        status += "\n:tv: **Watching**";
                        if (activity.Name != null)
                            status += " " + activity.Name;
                        status += "\n";
                    }
                    if (activity.ActivityType.Equals(ActivityType.Streaming))
                    {
                        status += "\n:satellite: **Streaming**";
                        if (activity.StreamUrl != null)
                            status += " on " + activity.StreamUrl;
                        status += "\n";
                    }
                }
            }
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    IconUrl = user.AvatarUrl,
                    Name = $"{user.Username}#{user.Discriminator}{(user.PremiumType == null ? "" : $" ({user.PremiumType} Sub)")}"
                },
                Color = color,
                Description = status
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("getuserstatuses"), RequireOwner, Hidden,
         Description("gets all server user statuses if they're online.")]
        public async Task GetStatuses(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync(":x: This command is only for server use.");
                return;
            }
            DiscordEmoji emojiUser = DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:");
            DiscordEmoji emojiBot = DiscordEmoji.FromName(ctx.Client, ":robot:");
            DiscordEmoji emojiOwner = DiscordEmoji.FromName(ctx.Client, ":crown:");
            DiscordEmoji emojiThisbot = DiscordEmoji.FromName(ctx.Client, ":gear:");
            var output = "";
            foreach ((ulong _, DiscordMember value) in ctx.Guild.Members)
            {
                output += $"{(value.IsOwner ? emojiOwner : "")}" +
                    $"{(value.IsBot ? (value.IsCurrent ? emojiThisbot : emojiBot) : (value.IsOwner ? "" : emojiUser))} `" +
                    $"{value.Username}#{value.Discriminator}` " +
                    $"{(value.Presence == null ? "is offline" : $"{(value.Presence.Activity == null ? "is online" : value.Presence.Activity.ActivityType == ActivityType.Custom ? $"is online with status {(value.Presence.Activity.CustomStatus.Emoji != null ? value.Presence.Activity.CustomStatus.Emoji.GetDiscordName() : "")} {value.Presence.Activity.CustomStatus.Name}" : value.Presence.Activity.ActivityType == ActivityType.Streaming ? $"streams on {value.Presence.Activity.StreamUrl}" : $"plays {value.Presence.Activity.Name}")}")}\n";
            }
            await ctx.RespondAsync($"{output}");
            if (ctx.Guild != null)
                await ctx.Message.DeleteAsync();
        }
    }
}
