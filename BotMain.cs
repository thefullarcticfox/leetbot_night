using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
// using DSharpPlus.Lavalink;
using DSharpPlus.VoiceNext;
using leetbot_night.Commands;
using leetbot_night.Config;
using leetbot_night.Cryptography;
using leetbot_night.Interactivities;
using leetbot_night.Math;
using leetbot_night.Monitors;
using leetbot_night.Services;
using Microsoft.Extensions.Logging;

namespace leetbot_night
{
	public class BotMain
	{
		// Assembly info strings
		internal static string			BotVersion;
		internal static string			BotConfig;
		internal static string			BotArch;
		// DSharpPlus stuff
		private DiscordClient			_discord;
		private CommandsNextExtension	_commands;
		// private InteractivityExtension	_interactivity;
		// private VoiceNextExtension		_voice;
		// private LavalinkExtension		_lavalink;
		// log channels
		private List<DiscordChannel>	_logChannels;
		private DateTime				_lastLogChWrite;
		// TwitchLib stuff
		private TwitchLiveMonitor		_tlm;
		private string					_ttvApIclid;
		private string					_ttvApIsecret;
		// Youtube V3 stuff
		private YoutubeVideoMonitor		_yvm;
		private string					_ytApIkey;
		// Crypto keys
		public static string AesKey { get; private set; }
		public static string AesIv { get; private set; }
		public static string RsaPublicKey { get; private set; }
		public static string RsaPrivateKey { get; private set; }

		public static void Main()
		{
			Console.OutputEncoding = Encoding.UTF8;
			var bot = new BotMain();
			try
			{
				bot.MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private async Task MainAsync()
		{
			// getting versions and assembly info
			BotVersion = FileVersionInfo.GetVersionInfo(Assembly.
				GetExecutingAssembly().Location).ProductVersion;
			BotConfig = Assembly.GetExecutingAssembly().GetCustomAttributes(true).
				OfType<AssemblyConfigurationAttribute>().FirstOrDefault()?.Configuration;
			BotArch = Assembly.GetExecutingAssembly().GetName().
				ProcessorArchitecture.ToString().ToLower();

			// init log channels
			_logChannels = new List<DiscordChannel>();
			_lastLogChWrite = File.GetLastWriteTime("logchannels.txt");

			string json = await FileHandler.ReadJsonConfig();
			if (json.Length == 0)
				return;
			if (json == "default")
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Created default config file.\n" +
								  "Now you need to get your discord bot token " +
								  "and put it in config.json file.\n" +
								  "Also make sure you set other parameters.");
				Console.ResetColor();
				return;
			}

			// setting up client
			var cfgjson = JsonSerializer.Deserialize<ConfigJson>(json);
			if (cfgjson == null)
				return;
			var cfg = new DiscordConfiguration
			{
				Token = cfgjson.Token,
				TokenType = TokenType.Bot,
				AutoReconnect = true,
				MinimumLogLevel = BotConfig == "Debug" ? LogLevel.Debug : LogLevel.Information,
				MessageCacheSize = 2048,
				LogTimestampFormat = "dd-MM-yyyy HH:mm:ss zzz"
			};

			// client init and event hooks
			_discord = new DiscordClient(cfg);
			_discord.Ready += Discord_Ready;
			_discord.GuildAvailable += Discord_GuildAvailable;
			_discord.GuildUnavailable += Discord_GuildUnavailable;
			_discord.GuildCreated += Discord_GuildCreated;
			_discord.GuildDeleted += Discord_GuildDeleted;
			_discord.ChannelDeleted += Discord_ChannelDeleted;
			_discord.DmChannelDeleted += Discord_DmChannelDeleted;
			_discord.GuildDownloadCompleted += Discord_GuildDownloadCompleted;
			_discord.ClientErrored += Discord_ClientErrored;
			_discord.SocketClosed += Discord_SocketClosed;
			_discord.Resumed += Discord_Resumed;
			_discord.Heartbeated += Discord_Heartbeated;

			// setting up interactivity
			var intcfg = new InteractivityConfiguration
			{
				Timeout = TimeSpan.FromMinutes(cfgjson.ActTimeout),
				PaginationDeletion = PaginationDeletion.DeleteMessage,
				PollBehaviour = PollBehaviour.KeepEmojis
			};
			_discord.UseInteractivity(intcfg);

			// setting up commands
			var cmdcfg = new CommandsNextConfiguration
			{
				StringPrefixes = new List<string> { cfgjson.CommandPrefix },
				EnableDms = cfgjson.DmsEnabled,
				EnableMentionPrefix = cfgjson.MentionEnabled,
				CaseSensitive = cfgjson.CaseSensitive,
				EnableDefaultHelp = true
			};
			// commands hooks and register
			_commands = _discord.UseCommandsNext(cmdcfg);
			_commands.CommandExecuted += Commands_CommandExecuted;
			_commands.CommandErrored += Commands_CommandErrored;
			_commands.RegisterCommands<Commands.Commands>();
			_commands.RegisterCommands<LeetCommands>();
			_commands.RegisterCommands<Interactivities.Interactivities>();
			_commands.RegisterCommands<Administrative>();
			_commands.RegisterCommands<Cats>();
			_commands.RegisterCommands<DiceRolling>();
			_commands.RegisterCommands<CryptoAes>();
			_commands.RegisterCommands<CryptoRsa>();
			_commands.RegisterCommands<MathCommands>();
			_commands.RegisterCommands<StatusCommands>();
			_commands.RegisterCommands<VoiceCommands>();
			// adding math converter for custom type and name
			var mathopscvrt = new MathOperationConverter();
			_commands.RegisterConverter(mathopscvrt);
			_commands.RegisterUserFriendlyTypeName<MathOperation>("operator");

			// setting up and enabling voice
			var vcfg = new VoiceNextConfiguration
			{
				AudioFormat = AudioFormat.Default,
				EnableIncoming = false
			};
			_discord.UseVoiceNext(vcfg);

			// setting custom help formatter
			_commands.SetHelpFormatter<HelpFormatter>();

			// init twitch live and youtube video monitors
			_ttvApIclid = cfgjson.TwitchApiClid;
			_ttvApIsecret = cfgjson.TwitchApiSecret;
			_ytApIkey = cfgjson.YoutubeApiKey;
			_tlm = new TwitchLiveMonitor();
			_yvm = new YoutubeVideoMonitor();

			// getting aes and rsa keys from config
			AesKey = cfgjson.AesKey;
			AesIv = cfgjson.AesIv;
			RsaPublicKey = cfgjson.RsaPublicKey;
			RsaPrivateKey = cfgjson.RsaPrivateKey;

			// connecting discord
			try
			{
				await _discord.ConnectAsync();
			}
			catch (Exception e)
			{
				_discord.Logger.LogCritical($"{e.Message}");
				return;
			}

			await Task.Delay(-1);
		}

		private Task Discord_Resumed(DiscordClient client, ReadyEventArgs e)
		{
			_tlm.StartMonitor();
			return Task.CompletedTask;
		}

		private Task Discord_SocketClosed(DiscordClient client, SocketCloseEventArgs e)
		{
			if (_tlm.MonitorRunning())
				_tlm.StopMonitor();
			return Task.CompletedTask;
		}

		private Task Discord_Ready(DiscordClient client, ReadyEventArgs e)
		{
			client.Logger.LogInformation($"Client v{BotVersion} {BotConfig}({BotArch}) ready.");

			// cut OS Description string
			string osString = RuntimeInformation.OSDescription;
			if (osString.Length > 32)
				while (osString.Contains("0123456789".ToCharArray()) &&
					   osString.LastIndexOfAny("0123456789".ToCharArray()) > 32)
					osString = osString.Remove(osString.LastIndexOfAny("0123456789".ToCharArray()));
			if (osString.Contains("0123456789".ToCharArray()))
				osString = osString.Substring(0, osString.LastIndexOfAny("0123456789".ToCharArray()) + 1);

			var game = new DiscordActivity
			{
				ActivityType = ActivityType.Playing,
				Name = (BotConfig != null ? BotConfig.ToLower() : "") + " v" + BotVersion + " on " + osString
			};
			_discord.UpdateStatusAsync(game, UserStatus.Online);
			return Task.CompletedTask;
		}

		private static Task Discord_GuildAvailable(DiscordClient client, GuildCreateEventArgs e)
		{
			client.Logger.LogInformation($"Guild available: {e.Guild.Name}");
			return Task.CompletedTask;
		}

		private static Task Discord_GuildUnavailable(DiscordClient client, GuildDeleteEventArgs e)
		{
			client.Logger.LogWarning($"Guild {e.Guild.Id} is unavailable (check Discord Status).");
			return Task.CompletedTask;
		}

		private static Task Discord_GuildCreated(DiscordClient client, GuildCreateEventArgs e)
		{
			client.Logger.LogInformation($"New guild: {e.Guild.Name}.");
			return Task.CompletedTask;
		}

		private Task Discord_GuildDeleted(DiscordClient client, GuildDeleteEventArgs e)
		{
			client.Logger.LogWarning($"Guild {e.Guild.Name} deleted.");

			List<string> channels = FileHandler.GetChannelListFromFile(_discord, "logchannels.txt");
			// check if there were log channels on this guild
			if (channels.FirstOrDefault(xch => xch.Contains($"{e.Guild.Id} ")) == null)
				return Task.CompletedTask;

			// delete them if there were
			channels.RemoveAll(xch => xch.Contains($"{e.Guild.Id} "));
			string output = channels.Aggregate("", (current, ch) => current + $"{ch}\n");
			Task.Run(async () => await File.WriteAllTextAsync("logchannels.txt", output));
			client.Logger.LogWarning("Log channels from this guild have been deleted.");
			return Task.CompletedTask;
		}

		private Task Discord_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
		{
			List<string> channels = FileHandler.GetChannelListFromFile(_discord, "logchannels.txt");
			string guildId = e.Channel.IsPrivate ? "user" : e.Channel.Guild.Id.ToString();
			if (!channels.Contains($"{guildId} {e.Channel.Id}"))
				return Task.CompletedTask;

			channels.Remove($"{guildId} {e.Channel.Id}");
			string output = channels.Aggregate("", (current, ch) => current + $"{ch}\n");
			Task.Run(async () => await File.WriteAllTextAsync("logchannels.txt", output));
			client.Logger.LogWarning($"{(guildId != "user" ? $"#{e.Channel.Name} on `{e.Channel.Guild.Name}`" : $"DM channel {e.Channel.Id}")} " +
									   "has been deleted from log channels of this bot because this channel was deleted.");
			return Task.CompletedTask;
		}

		private Task Discord_DmChannelDeleted(DiscordClient client, DmChannelDeleteEventArgs e)
		{
			List<string> channels = FileHandler.GetChannelListFromFile(_discord, "logchannels.txt");
			if (!channels.Contains($"user {e.Channel.Id}"))
				return Task.CompletedTask;

			channels.Remove($"user {e.Channel.Id}");
			string output = channels.Aggregate("", (current, ch) => current + $"{ch}\n");
			Task.Run(async () => await File.WriteAllTextAsync("logchannels.txt", output));
			client.Logger.LogWarning($"DM channel {e.Channel.Id} has been deleted from " +
				"log channels of this bot because this channel was deleted.");
			return Task.CompletedTask;
		}

		private Task Discord_GuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs e)
		{
			client.Logger.LogInformation("Guilds ready. Loading log channels.");

			// getting log channels
			_logChannels = Task.Run(async () => await FileHandler.GetLogChannelsAsync(_discord)).Result;

			foreach (DiscordChannel logChannel in _logChannels)
			{
				client.Logger.LogInformation("Log channel available: " +
					$"{(logChannel.IsPrivate ? $"DM Channel {logChannel.Id}" : $"{logChannel.Name} on {logChannel.Guild.Name}")}");
			}

			// run twitch live monitor (check interval in seconds)
			if (!_tlm.MonitorRunning() &&
				_ttvApIclid.Length == 30 &&
				_ttvApIsecret.Length == 30)
				_tlm.RunMonitor(_discord, _ttvApIclid, _ttvApIsecret, _logChannels, 60);
			if (_ttvApIclid.Length != 30 &&
				_ttvApIsecret.Length == 30)
				client.Logger.LogWarning("TwitchLiveMonitor not launched (wrong client id or secret format). Check config.json file.");

			// run youtube video monitor (check interval in minutes)
			// with 5 minutes interval every day max quota will be 600
			if (!_yvm.MonitorRunning() &&
				_ytApIkey.Length == 39)
				_yvm.RunService(_discord, _ytApIkey, _logChannels, 5);
			if (_ytApIkey.Length != 39)
				client.Logger.LogWarning("YoutubeService not launched (wrong API Key format). Check config.json file.");

			// send ready message to the first log channel on the first guild
			try
			{
				DiscordChannel firstLogChannel = _logChannels[0];
				_discord.SendMessageAsync(firstLogChannel,
					$"Client v{BotVersion} {BotConfig}({BotArch}) ready.");
			}
			catch
			{
				// ignored
			}

			return Task.CompletedTask;
		}

		private Task Discord_Heartbeated(DiscordClient client, HeartbeatEventArgs e)
		{
			// check if log channels list updated
			if (DateTime.Compare(File.GetLastWriteTime("logchannels.txt"), _lastLogChWrite) <= 0)
				return Task.CompletedTask;

			_logChannels.Clear();
			// update log discord channels
			_logChannels = Task.Run(async () => await FileHandler.GetLogChannelsAsync(_discord)).Result;
			_lastLogChWrite = File.GetLastWriteTime("logchannels.txt");

			_tlm.UpdateLogChannels(_logChannels);
			_yvm.UpdateLogChannels(_logChannels);
			_discord.Logger.LogInformation("Log channels updated.");
			return Task.CompletedTask;
		}

		private static Task Discord_ClientErrored(DiscordClient client, ClientErrorEventArgs e)
		{
			client.Logger.LogError(e.Exception, e.Exception.Message);
			/*	e.Client.DebugLogger.LogMessage(LogLevel.Error, "leetbot",
					$"Exception occured: {e.Exception.GetType()}:" +
					$"{e.Exception.Message}", DateTime.Now);		*/
			return Task.CompletedTask;
		}

		private static Task Commands_CommandExecuted(CommandsNextExtension cnext, CommandExecutionEventArgs e)
		{
			cnext.Client.Logger.LogInformation($"{e.Context.User.Username} successfully executed " +
												$"'{e.Command.QualifiedName}'");
			return Task.CompletedTask;
		}

		private async Task Commands_CommandErrored(CommandsNextExtension cnext, CommandErrorEventArgs e)
		{
			if (e.Exception is CommandNotFoundException &&
				(e.Command == null || e.Command.QualifiedName != "help"))
				return;				// ignoring misspelled commands

			cnext.Client.Logger.LogWarning($"{e.Context.User.Username}#{e.Context.User.Discriminator} tried executing " +
											$"'{e.Command.QualifiedName ?? "<unknown command>"}' with error " +
											$"{e.Exception.GetType()}: {e.Exception.Message}");

			var embed = new DiscordEmbedBuilder {Color = new DiscordColor(0xFF0000)};
			switch (e.Exception)
			{
				case ChecksFailedException cfex:
					embed.Title = ":no_entry: No access";
					embed.Description = "You (or bot) don't have permissions to execute command" +
										$"`!{cfex.Command.QualifiedName}`.";
					break;
				case ArgumentException:
				{
					embed.Title = ":x: Not enough arguments or wrong argument type";
					embed.Description = $"Command string: `!{e.Command.QualifiedName}" +
										$"{string.Join("", e.Command.Overloads[0].Arguments.Select(xarg => $" <{xarg.Name}>")!)}`\n";
					if (e.Command.Overloads[0].Arguments.Any())
					{
						string arglist = string.Join("\n", e.Command.Overloads[0].Arguments.Select(xarg =>
							!xarg.IsOptional ? $"`{xarg.Name}` ({e.Context.CommandsNext.GetUserFriendlyTypeName(xarg.Type)}) - " +
											   $"{xarg.Description ?? "no description provided."}" : "")!);
						embed.AddField("Required arguments", arglist);
					}
					if (e.Command.ToString().Contains("Command Group"))
					{
						embed.Title = ":x: Subcommand not found";
						embed.Description = $"Command group `!{e.Command.QualifiedName}`";
					}
					embed.AddField("Help", $"Check `!help {e.Command.QualifiedName}` for more info.");
					break;
				}
				default:
				{
					embed.Title = ":x: Unknown exception occured";
					embed.Description = $"{e.Exception.Message}";
					if (e.Exception.InnerException != null)
					{
						Exception innerex = e.Exception.InnerException;
						while (innerex != null)
						{
							embed.Description += " -> " + innerex.Message;
							innerex = innerex.InnerException;
						}
					}
					embed.AddField("Help", $"Check `!help {e.Command.QualifiedName}` for more info.");
					break;
				}
			}
			await e.Context.RespondAsync(embed: embed.Build());
		}
	}
}
