using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using leetbot_night.Services;

namespace leetbot_night.Commands
{
	[Group("admin"), Hidden,
	 Description("Administrative commands.")]
	public class Administrative : BaseCommandModule
	{
		[GroupCommand]
		public async Task ExecuteGroupAsync(CommandContext ctx)
		{
			await ctx.RespondAsync("Check `!help admin` for command usage.");
		}

		[Command("sudo"), Hidden, RequireOwner,
		 Description("executes a command as another user.")]
		public async Task Sudo(CommandContext ctx,
			[Description("user to execute as.")] DiscordMember member,
			[Description("command text to execute."), RemainingText] string command)
		{
			if (ctx.Channel.IsPrivate)
			{
				await ctx.RespondAsync(":x: This command is only for server use.");
				return;
			}
			await ctx.TriggerTypingAsync();
			CommandsNextExtension cmds = ctx.CommandsNext;
			CommandContext fakectx = cmds.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmds.FindCommand(command, out _));
			await cmds.ExecuteCommandAsync(fakectx);
		}

		[Command("nick"), RequirePermissions(Permissions.ChangeNickname),
		 Description("changes someone's nickname on server.")]
		public async Task ChangeNickname(CommandContext ctx,
			[Description("member to change the nickname for.")] DiscordMember member,
			[Description("the nickname to give to that user."), RemainingText] string newNickname)
		{
			if (ctx.Channel.IsPrivate)
			{
				await ctx.RespondAsync(":x: This command is only for server use.");
				return;
			}
			await ctx.TriggerTypingAsync();
			try
			{
				await member.ModifyAsync(memberEditModel => memberEditModel.Nickname = newNickname);
				await ctx.RespondAsync("Done.");
			}
			catch (Exception)
			{
				await ctx.RespondAsync("Operation failed.");
			}
		}

		[Command("kick"), RequirePermissions(Permissions.KickMembers),
		 Description("kicks someone from the server.")]
		public async Task KickMember(CommandContext ctx,
			[Description("member to kick.")] DiscordMember member)
		{
			if (ctx.Channel.IsPrivate)
			{
				await ctx.RespondAsync(":x: This command is only for server use.");
				return;
			}
			await ctx.TriggerTypingAsync();
			try
			{
				await member.RemoveAsync($"Kicked by {ctx.User.Username} ({ctx.User.Id}).");
				await ctx.RespondAsync("Done.");
			}
			catch (Exception)
			{
				await ctx.RespondAsync("Operation failed.");
			}
		}

		[Command("ban"), RequirePermissions(Permissions.BanMembers),
		 Description("bans someone from the server.")]
		public async Task BanMember(CommandContext ctx,
			[Description("member to ban.")] DiscordMember member,
			[Description("how many days to delete messages from.")] int deletedays)
		{
			if (ctx.Channel.IsPrivate)
			{
				await ctx.RespondAsync(":x: This command is only for server use.");
				return;
			}
			await ctx.TriggerTypingAsync();
			try
			{
				await member.BanAsync(deletedays, $"Banned by {ctx.User.Username} ({ctx.User.Id}).");
				await ctx.RespondAsync("Done.");
			}
			catch (Exception)
			{
				await ctx.RespondAsync("Operation failed.");
			}
		}

		[Command("getemotes"), Hidden, RequireOwner,
		 Description("gets all custom emotes on the server.")]
		public async Task GetEmotes(CommandContext ctx,
			[Description("default: emotes not managed by integration.\n" +
						 "`all` - for all emotes.\n" +
						 "`managedonly` - only for managed by integration emotes.")] string mode = "")
		{
			if (ctx.Channel.IsPrivate)
			{
				await ctx.RespondAsync(":x: This command is only for server use.");
				return;
			}

			await ctx.TriggerTypingAsync();
			IReadOnlyDictionary<ulong, DiscordEmoji> emotes = ctx.Guild.Emojis;
			string ffguildname = ctx.Guild.Name		// folder friendly guild name
				.Replace("\\", "")
				.Replace("\'", "")
				.Replace("\"", "")
				.Replace("/", "")
				.Replace(":", "")
				.Replace("*", "")
				.Replace("?", "")
				.Replace("<", "")
				.Replace(">", "")
				.Replace("|", "");

			foreach ((ulong _, DiscordEmoji discordEmoji) in emotes)
			{
				if (mode == "all" || (mode == "managedonly" ? discordEmoji.IsManaged : !discordEmoji.IsManaged))
				{
					await FileHandler.DownloadUrlFile($"https://cdn.discordapp.com/emojis/{discordEmoji.Name}.png",
						$"D:\\source\\emotes\\{ffguildname}\\", ctx.Client, $"{discordEmoji.Name}.png");
				}
			}
			await ctx.RespondAsync("Emotes downloaded.");
			if (ctx.Guild != null)
				await ctx.Message.DeleteAsync();
		}

		[Command("leave"), RequireOwner,
		 Description("leaves server in 20 seconds.")]
		public async Task LeaveServer(CommandContext ctx)
		{
			if (ctx.Channel.IsPrivate)
			{
				await ctx.RespondAsync(":x: This command is only for server use.");
				return;
			}
			InteractivityExtension interactivity = ctx.Client.GetInteractivity();
			var embed = new DiscordEmbedBuilder
			{
				Title = ":x: Server leave sequence launched",
				Description = "Leaving the server in 20 seconds.\n" +
							  "Enter `dont` to stop the countdown.",
				Color = new DiscordColor(0xFF0000)
			};
			await ctx.RespondAsync(embed: embed);
			await ctx.TriggerTypingAsync();     // to show we're working

			InteractivityResult<DiscordMessage> stopmsg = await interactivity.WaitForMessageAsync(xm =>
				xm.ChannelId.Equals(ctx.Message.ChannelId) &&
				xm.Author.Equals(ctx.Message.Author) &&
				xm.Content == "dont",
				TimeSpan.FromSeconds(20));
			if (stopmsg.TimedOut)
			{
				await ctx.RespondAsync("Well, it was a good time.\n" +
									   "Leaving server now.");
				await ctx.Guild.LeaveAsync();
			}
			else
				await ctx.RespondAsync("okay. i'll stay.");
		}

		[Group("emote"), RequirePermissions(Permissions.ManageEmojis),
		 Description("Emote management.")]
		public class Emotes : BaseCommandModule
		{
			[GroupCommand]
			public async Task ExecuteGroupAsync(CommandContext ctx)
			{
				await ctx.RespondAsync("Check `!help admin emote` for command usage.");
			}

			[Command("get"), Hidden, RequireOwner,
			 Description("downloads a custom emote.")]
			public async Task GetEmote(CommandContext ctx,
				[Description("emote to get.")] DiscordEmoji emote)
			{
				await ctx.TriggerTypingAsync();
				await FileHandler.DownloadUrlFile($"https://cdn.discordapp.com/emojis/{emote.Id}.png",
					"D:\\source\\emotes\\", ctx.Client, $"{emote.Name}.png");
				await ctx.RespondAsync($"Emote {emote} downloaded.");
				if (ctx.Guild != null)
					await ctx.Message.DeleteAsync();
			}

			[Command("create"),
			 Description("creates a custom emote from attached image.")]
			public async Task CreateEmote(CommandContext ctx,
				[Description("emote name.\n" +
							 "Default: will give a png filename if empty.")] string emoteName = "")
			{
				if (ctx.Channel.IsPrivate)
				{
					await ctx.RespondAsync(":x: This command is only for server use.");
					return;
				}

				var emotepath = "D:\\source\\emotes\\temp\\";
				await ctx.TriggerTypingAsync();
				if (ctx.Message.Attachments == null)
					await ctx.RespondAsync("No new emote attached. Cancelling.");
				else
				{
					try
                    {
						DiscordAttachment newemote = ctx.Message.Attachments[0];
						string filetype = newemote.FileName.Substring(newemote.FileName.LastIndexOf(".", StringComparison.Ordinal), 4);
						bool isImage = filetype is ".png" or ".gif";
						if (!isImage)
							await ctx.RespondAsync("Wrong file type. Cancelling.");
						else if (newemote.FileSize > 256 * 1024)
							await ctx.RespondAsync("New emote file size more that 256Kb. Cancelling.");
						else
						{
							if (emoteName.Length == 0)
								emoteName = newemote.FileName.Substring(0, newemote.FileName.LastIndexOf(".", StringComparison.Ordinal));
							await FileHandler.DownloadUrlFile(newemote.Url, emotepath, ctx.Client, $"{emoteName}{filetype}");
							await Task.Delay(1000);     // little delay for our file stream
							await using (FileStream fileStream = File.OpenRead($"{emotepath}{emoteName}{filetype}"))
							{
								DiscordGuildEmoji createdEmote = await ctx.Guild.CreateEmojiAsync(emoteName, fileStream);
								await ctx.RespondAsync($"Created new emote: {createdEmote}");
								fileStream.Close();
							}
							File.Delete($"{emotepath}{emoteName}{filetype}");
						}
                    }
					catch
					{
						// ignored
					}
				}
			}

			[Command("delete"),
			 Description("deletes a custom emote.")]
			public async Task DeleteEmote(CommandContext ctx,
				[Description("emote to delete.")] DiscordEmoji emote)
			{
				if (ctx.Channel.IsPrivate)
				{
					await ctx.RespondAsync(":x: This command is only for server use.");
					return;
				}

				await ctx.TriggerTypingAsync();
				DiscordGuildEmoji guildemote = null;
				if (emote.IsManaged)
					await ctx.RespondAsync("You can't delete an integration-managed emote.");
				else
				{
					if (ctx.Guild.Emojis.Any(guildemoji => guildemoji.Value == emote))
						guildemote = ctx.Guild.GetEmojiAsync(emote.Id).Result;
					if (guildemote == null)
						await ctx.RespondAsync("This emote is not from this server.");
					else
					{
						await FileHandler.DownloadUrlFile($"https://cdn.discordapp.com/emojis/{guildemote.Id}.png",
							"D:\\source\\emotes\\backup\\", ctx.Client, $"{guildemote.Name}.png");
						await ctx.Guild.DeleteEmojiAsync(guildemote, $"Emote {guildemote.Name} deleted by {ctx.User.Username} ({ctx.User.Id}).");
						await ctx.RespondAsync($"Emote {guildemote.Name} deleted.");
					}
				}
			}

			[Command("replace"),
			 Description("replaces a custom emote.")]
			public async Task ReplaceEmote(CommandContext ctx,
				[Description("emote to replace.")] DiscordEmoji emote,
				[Description("new emote name.\n" +
							 "Default: emote name stays the same.")] string newEmoteName = "")
			{
				if (ctx.Channel.IsPrivate)
				{
					await ctx.RespondAsync(":x: This command is only for server use.");
					return;
				}
				await ctx.TriggerTypingAsync();
				if (newEmoteName.Length == 0)
					newEmoteName = emote.Name;
				await DeleteEmote(ctx, emote);
				await CreateEmote(ctx, newEmoteName);
				await ctx.RespondAsync("Emote successfully replaced.");
			}

			[Command("rename"),
			 Description("renames a custom emote.")]
			public async Task RenameEmote(CommandContext ctx,
				[Description("emote to rename.")] DiscordEmoji emote,
				[Description("new emote name.")] string newEmoteName)
			{
				if (ctx.Channel.IsPrivate)
				{
					await ctx.RespondAsync(":x: This command is only for server use.");
					return;
				}
				await ctx.TriggerTypingAsync();
				DiscordGuildEmoji guildemote = null;
				if (emote.IsManaged)
					await ctx.RespondAsync("You can't modify an integration-managed emote.");
				else
				{
					if (ctx.Guild.Emojis.Any(guildemoji => guildemoji.Value == emote))
						guildemote = ctx.Guild.GetEmojiAsync(emote.Id).Result;
					if (guildemote == null)
						await ctx.RespondAsync("This emote is not from this server.");
					else
					{
						await ctx.Guild.ModifyEmojiAsync(guildemote, newEmoteName, reason: $"Emote name changed to {newEmoteName} by {ctx.User.Username} ({ctx.User.Id}). Was: {guildemote.Name}");
						await ctx.RespondAsync($"Emote {guildemote} modified.");
					}
				}
			}
		}

		[Group("logging"), RequirePermissions(Permissions.ManageChannels),
		 Description("TwitchLive logs management.")]
		public class Logging : BaseCommandModule
		{
			[GroupCommand]
			public async Task ExecuteGroupAsync(CommandContext ctx)
			{
				await ctx.RespondAsync("Check `!help admin logging` for command usage.");
			}

			[Command("addlogchannel"),
			 Description("adds current channel to the list of log channels.")]
			public async Task AddLogChannel(CommandContext ctx,
				[Description("channel to add. (current channel if not specified)")] DiscordChannel discordChannel = null)
			{
				if (discordChannel == null)
					discordChannel = ctx.Channel;
				List<string> channels = FileHandler.GetChannelListFromFile(ctx.Client, "logchannels.txt");
				string guildId;
				try
				{
					guildId = discordChannel.IsPrivate ? "user" : discordChannel.Guild.Id.ToString();
				}
				catch (NullReferenceException)
				{
					await ctx.RespondAsync(":x: The channel you entered does not exist.");
					return;
				}

				if (channels.Contains($"{guildId} {discordChannel.Id}"))
					await ctx.RespondAsync("This channel is already in log channels.");
				else
				{
					await File.AppendAllTextAsync("logchannels.txt", $"{guildId} {discordChannel.Id}\n");
					await ctx.RespondAsync($"{(guildId != "user" ? $"{discordChannel.Mention} on `{discordChannel.Guild.Name}`" : "This DM channel")} " +
										   "has been added to log channels of this bot.\n" +
										   "Now bot will send twitch live notifications to this channel with here mention.");
				}
			}

			[Command("deletelogchannel"),
			 Description("deletes channel from the list of log channels.")]
			public async Task DeleteLogChannel(CommandContext ctx,
				[Description("channel to delete. (current channel if not specified)")] DiscordChannel discordChannel = null)
			{
				if (discordChannel == null)
					discordChannel = ctx.Channel;
				List<string> channels = FileHandler.GetChannelListFromFile(ctx.Client, "logchannels.txt");
				string guildId;

				try
				{
					guildId = discordChannel.IsPrivate ? "user" : discordChannel.Guild.Id.ToString();
				}
				catch (NullReferenceException)
				{
					await ctx.RespondAsync(":x: The channel you entered does not exist.");
					return;
				}
				if (channels.Contains($"{guildId} {discordChannel.Id}"))
				{
					channels.Remove($"{guildId} {discordChannel.Id}");
					string output = channels.Aggregate("", (current, ch) => current + $"{ch}\n");
					await File.WriteAllTextAsync("logchannels.txt", output);
					await ctx.RespondAsync($"{(guildId != "user" ? $"{discordChannel.Mention} on `{discordChannel.Guild.Name}`" : "This DM channel")} " +
										   "has been deleted from log channels of this bot.");
				}
				else
					await ctx.RespondAsync("This channel is not in log channels.");
			}

			[Command("logchannels"), Hidden, RequireOwner,
			 Description("list of log channels.")]
			public async Task LogChannelList(CommandContext ctx)
			{
				List<string> channels = FileHandler.GetChannelListFromFile(ctx.Client, "logchannels.txt");
				var output = "";

				foreach (string[] chchain in channels.Select(ch => ch.Split(' ')))
				{
					DiscordChannel dch;
					if (chchain[0] == "user")
					{
						dch = await ctx.Client.GetChannelAsync(Convert.ToUInt64(chchain[1]));
						output += $"DM Channel `{dch.Id}`\n";
					}
					else
					{
						DiscordGuild guild = await ctx.Client.GetGuildAsync(Convert.ToUInt64(chchain[0]));
						dch = guild.GetChannel(Convert.ToUInt64(chchain[1]));
						output += $"{dch.Mention} on `{dch.Guild.Name}`{(guild.IsUnavailable ? " (unavailable)" : "")}\n";
					}
				}
				var embed = new DiscordEmbedBuilder
				{
					Color = new DiscordColor(0x6441a5),
					Description = output,
					Title = ":pencil: TwitchLive Log Channels"
				};
				await ctx.RespondAsync(embed: embed);
				if (ctx.Guild != null)
					await ctx.Message.DeleteAsync();
			}

			[Command("addtwitchchannel"), Hidden, RequireOwner,
			 Description("adds current channel to the list of twitch channels.")]
			public async Task AddTwitchChannel(CommandContext ctx,
				[Description("channel to add.")] string twitchChannel)
			{
				List<string> channels = FileHandler.GetChannelListFromFile(ctx.Client, "twitchchannels.txt");

				if (channels.Contains($"{twitchChannel}"))
					await ctx.RespondAsync("This channel is already in twitch channels.");
				else
				{
					await File.AppendAllTextAsync("twitchchannels.txt", $"{twitchChannel}\n");
					await ctx.RespondAsync($"`{twitchChannel}` has been added to twitch channels of this bot.");
				}
			}

			[Command("deletetwitchchannel"), Hidden, RequireOwner,
			 Description("deletes channel from the list of twitch channels.")]
			public async Task DeleteTwitchChannel(CommandContext ctx,
				[Description("channel to delete.")] string twitchChannel)
			{
				List<string> channels = FileHandler.GetChannelListFromFile(ctx.Client, "twitchchannels.txt");

				if (channels.Contains($"{twitchChannel}"))
				{
					channels.Remove($"{twitchChannel}");
					string output = channels.Aggregate("", (current, ch) => current + $"{ch}\n");
					await File.WriteAllTextAsync("twitchchannels.txt", output);
					await ctx.RespondAsync($"`{twitchChannel}` has been deleted from twitch channels of this bot.\n");
				}
				else
					await ctx.RespondAsync("This channel is not in twitch channels.");
			}

			[Command("twitchchannels"),
			 Description("list of logged twitch channels.")]
			public async Task TwitchChannelList(CommandContext ctx)
			{
				List<string> channels = FileHandler.GetChannelListFromFile(ctx.Client, "twitchchannels.txt");
				string output = channels.Aggregate("", (current, ch) => current + $"{ch}\n");
				var embed = new DiscordEmbedBuilder
				{
					Color = new DiscordColor(0x6441a5),
					Description = output,
					Title = ":tv: Monitored Twitch Channels"
				};
				await ctx.RespondAsync(embed: embed);
				if (ctx.Guild != null)
					await ctx.Message.DeleteAsync();
			}
		}
	}
}
