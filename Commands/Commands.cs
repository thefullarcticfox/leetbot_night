using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace leetbot_night.Commands
{
	public class Commands : BaseCommandModule
	{
		[Command("random"),
		 Description("random number generator.")]
		public async Task Random(CommandContext ctx,
			[Description("min number.")] int min,
			[Description("max number.")] int max)
		{
			var rnd = new Random();
			await ctx.RespondAsync($"Your random number is: {rnd.Next(min, max)}");
		}

		[Command("rate"),
		 Description("rates something randomly.")]
		public async Task RateRng(CommandContext ctx,
			[RemainingText] string something = "")
		{
			var		rndrateint = new Random();
			var		rndratedouble = new Random();
			double	rndrate = rndrateint.Next(0, 11);
			if (System.Math.Abs(rndrate - 10) > 0)
				rndrate += System.Math.Round(rndratedouble.NextDouble(), 2);
			var	output = "";
			if (something.Length > 0)
				output = $"{something} is ";
			output += rndrate + "/10";
			await ctx.RespondAsync(output);
		}

		[Command("info"),
		 Description("random percentage of info probability.")]
		public async Task Random(CommandContext ctx,
			[RemainingText] string infa = "")
		{
			var rndpercent = new Random();
			if (infa.Length > 0)
				await ctx.RespondAsync($"\"{infa}\" - {rndpercent.Next(0, 100)}% info.");
			else
				await ctx.RespondAsync($"Info {rndpercent.Next(0, 100)}%");
		}

		[Command("screenshare"), RequireGuild,
		 Description("sends screenshare link for the chat you're in.")]
		public async Task GetScreenShareLink(CommandContext ctx,
			[Description("chat name."), RemainingText] DiscordChannel chn = null)
		{
			DiscordVoiceState vstat = ctx.Member?.VoiceState;
			if (vstat?.Channel == null && chn == null)
			{
				await ctx.RespondAsync("You are not in a voice channel.");
				return;
			}
			if (chn == null)
				chn = vstat.Channel;
			await ctx.RespondAsync($"https://discordapp.com/channels/{chn.GuildId}/{chn.Id}");
		}

		[Command("members"), Hidden,
		 Description("gets guild all members.")]
		public async Task GetUsers(CommandContext ctx,
			[Description("default: all members.\n" +
						 "Modes:\n" +
						 "`usersonly` - users only.\n" +
						 "`onlineusersonly` - online users only.\n" +
						 "`botsonly` - bots only."), RemainingText] string mode)
		{
			if (ctx.Channel.IsPrivate)
			{
				await ctx.RespondAsync(":x: This command is only for server use.");
				return;
			}
			DiscordEmoji emojiUser = DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:");
			DiscordEmoji emojiUsers = DiscordEmoji.FromName(ctx.Client, ":busts_in_silhouette:");
			DiscordEmoji emojiBot = DiscordEmoji.FromName(ctx.Client, ":robot:");
			DiscordEmoji emojiOwner = DiscordEmoji.FromName(ctx.Client, ":crown:");
			DiscordEmoji emojiThisbot = DiscordEmoji.FromName(ctx.Client, ":gear:");
			var embedtitle = "Members";
			var output = "";
			switch (mode)
			{
				case "usersonly":
				{
					foreach ((ulong _, DiscordMember value) in ctx.Guild.Members)
					{
						output += $"{(value.IsBot ? "" : $"{(value.IsOwner ? emojiOwner : emojiUser)} `{value.Username}#{value.Discriminator}` {(value.DisplayName.Equals(value.Username) ? "" : $" aka `{value.DisplayName}`")}\n")}";
					}
					embedtitle = "Users";
					break;
				}
				case "onlineusersonly":
				{
					foreach ((ulong _, DiscordMember value) in ctx.Guild.Members)
					{
						output += $"{(value.IsBot || value.Presence == null ? "" : $"{(value.IsOwner ? emojiOwner : emojiUser)} `{value.Username}#{value.Discriminator}` {(value.DisplayName.Equals(value.Username) ? "" : $" aka `{value.DisplayName}`")}\n")}";
					}
					embedtitle = "Online Users";
					break;
				}
				case "botsonly":
				{
					foreach ((ulong _, DiscordMember value) in ctx.Guild.Members)
					{
						output += $"{(value.IsBot ? $"{(value.IsCurrent ? emojiThisbot : emojiBot)} `{value.Username}#{value.Discriminator}` {(value.DisplayName.Equals(value.Username) ? "" : $" aka `{value.DisplayName}`")}\n" : "")}";
					}
					embedtitle = "Bots";
					break;
				}
				default:
				{
					foreach ((ulong _, DiscordMember value) in ctx.Guild.Members)
					{
						output += $"{(value.IsOwner ? emojiOwner : "")}" +
								  $"{(value.IsBot ? (value.IsCurrent ? emojiThisbot : emojiBot) : (value.IsOwner ? "" : emojiUser))} " +
								  $"`{value.Username}#{value.Discriminator}` " +
								  $"{(value.DisplayName.Equals(value.Username) ? "" : $" aka `{value.DisplayName}`")}\n";
					}
					break;
				}
			}
			var embed = new DiscordEmbedBuilder
			{
				Color = new DiscordColor(0xEB4910),
				Title = $"{emojiUsers} {embedtitle} of {ctx.Guild.Name}",
				Description = output,
				Footer = new DiscordEmbedBuilder.EmbedFooter
				{
					Text = $"Users (Online): {ctx.Guild.Members.Count(u => !u.Value.IsBot)} " +
					$"({ctx.Guild.Members.Count(u => !u.Value.IsBot && u.Value.Presence != null)}). " +
					$"Bots: {ctx.Guild.Members.Count(u => u.Value.IsBot)}.\n" +
					$"Total: {ctx.Guild.Members.Count} members. "
				}
			};
			await ctx.RespondAsync(embed: embed);
			if (ctx.Guild != null)
				await ctx.Message.DeleteAsync();
		}

		[Command("servers"), RequireOwner, Hidden,
		 Description("gets guilds/servers list where this bot was invited to.")]
		public async Task GetGuilds(CommandContext ctx)
		{
			DiscordEmoji emojiGuild = DiscordEmoji.FromName(ctx.Client, ":busts_in_silhouette:");
			DiscordEmoji emojiBigguild = DiscordEmoji.FromName(ctx.Client, ":gem:");
			DiscordEmoji emojiGuilds = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
			var output = "";
			foreach (KeyValuePair<ulong, DiscordGuild> guild in ctx.Client.Guilds)
			{
				output += $"{(guild.Value.IsLarge ? emojiBigguild : emojiGuild)} **{guild.Value.Name}**\n" +
					$"Members: {guild.Value.MemberCount}\n" +
					$"Joined: {guild.Value.JoinedAt}\n" +
					$"Created: {guild.Value.CreationTimestamp}\n" +
					$"Owner: `{guild.Value.Owner.Username}#{guild.Value.Owner.Discriminator}`\n";
			}
			var embed = new DiscordEmbedBuilder
			{
				Color = new DiscordColor(0xEB4910),
				Title = $"{emojiGuilds} Servers",
				Description = output,
				Footer = new DiscordEmbedBuilder.EmbedFooter
				{
					Text = $"Total: {ctx.Client.Guilds.Count} servers."
				}
			};
			await ctx.RespondAsync(embed: embed);
			if (ctx.Guild != null)
				await ctx.Message.DeleteAsync();
		}

		[Command("version"),
		 Description("bot version. Use mode `changelog` to get latest changelog.")]
		public async Task GetVersion(CommandContext ctx,
			string mode = "null")
		{
			if (mode == "changelog")
			{
				string changelog;
				await using (FileStream fs = File.OpenRead("changelog.txt"))
				using (var sr = new StreamReader(fs, new UTF8Encoding(true)))
					changelog = await sr.ReadToEndAsync();
				await ctx.Channel.SendMessageAsync($"Current version {BotMain.BotVersion}\n" +
												   $"```{changelog}```");
			}
			string twitchlibversion = FileVersionInfo.GetVersionInfo("TwitchLib.Api.Core.dll").ProductVersion;
			string youtubeapiversion = FileVersionInfo.GetVersionInfo("Google.Apis.YouTube.v3.dll").FileVersion;
			var embed = new DiscordEmbedBuilder
			{
				Color = new DiscordColor(0x000000),
				Description = $"**bot version** {BotMain.BotVersion}\n"
							+ $"**build time**  {File.GetLastWriteTime("leetbot_night.dll")} " +
							  $"(UTC{(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow) >= TimeSpan.Zero ? "+" : "-")}" +
							  $"{TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow):hh\\:mm})\n"
							+ "\n**Uses:**\n"
							+ $"DSharp+ v{ctx.Client.VersionString}\n"
							+ $"TwitchLib v{twitchlibversion}\n"
							+ $"Youtube Data API v3 v{youtubeapiversion}",
				Author = new DiscordEmbedBuilder.EmbedAuthor
				{
					Name = $"{ctx.Client.CurrentUser.Username} {BotMain.BotArch} {BotMain.BotConfig}",
					IconUrl = ctx.Client.CurrentUser.AvatarUrl
				},
				Footer = new DiscordEmbedBuilder.EmbedFooter
				{
					Text = "// created by thefullarcticfox_",
					IconUrl = null
				}
			};
			await ctx.Channel.SendMessageAsync(embed: embed);
			if (ctx.Guild != null)
				await ctx.Message.DeleteAsync();
		}
	}
}
