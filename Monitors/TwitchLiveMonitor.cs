using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using leetbot_night.Services;
using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace leetbot_night.Monitors
{
	public class TwitchLiveMonitor
	{
		private DiscordClient				_discordClient;
		private TwitchAPI					_api;
		private LiveStreamMonitorService	_tlm;
		private List<DiscordChannel>		_logChannelsList;
		private DateTime					_lastTwChWrite;
		private List<string>				_twitchChannelList;
		private bool						_firstLaunch = true;
		private string						_apiClientId;
		private string						_apiSecret;

		public virtual void RunMonitor(DiscordClient client,
			string clientId, string clientSecret,
			List<DiscordChannel> discordChannels, int checkInterval)
		{
			_discordClient = client;
			_logChannelsList = discordChannels;                  // log channels list
			_twitchChannelList = new List<string>();             // init twitch channels list
			_apiClientId = clientId;
			_apiSecret = clientSecret;
			Task.Run(() => ConfigTlmAsync(checkInterval));      // run monitor
		}

		public virtual bool MonitorRunning()
		{
			return _tlm is {Enabled: true};
		}

		public virtual void StartMonitor()
		{
			if (!_tlm.Enabled)
				_tlm.Start();
		}

		public virtual void StopMonitor()
		{
			if (_tlm.Enabled)
				_tlm.Stop();
		}

		public virtual void UpdateLogChannels(List<DiscordChannel> discordChannels)
		{
			_logChannelsList.Clear();
			_logChannelsList = discordChannels;
		}

		private async Task ConfigTlmAsync(int checkInterval)
		{
			try
			{
				_api = new TwitchAPI();
				_api.Settings.ClientId = _apiClientId;
				_api.Settings.Secret = _apiSecret;
				_tlm = new LiveStreamMonitorService(_api, checkInterval);
				if (!GetTwitchChannelsAsync()) // get twitch channels to list
					return;
				_tlm.SetChannelsByName(_twitchChannelList);
				_tlm.OnServiceStarted += TLM_OnServiceStarted;
				_tlm.OnServiceStopped += TLM_OnServiceStopped;
				_tlm.OnStreamOnline += TLM_OnStreamOnline;
				_tlm.OnStreamOffline += TLM_OnStreamOffline;
				_tlm.OnServiceTick += TLM_OnServiceTick;
				_tlm.Start();
				await Task.Delay(-1);
			}
			catch (Exception e)
			{
				_discordClient.Logger
					.LogError($"[TwitchLiveMonitor] {e.Message} Restarting in 1 minute.");
				await Task.Delay(60 * 1000);
				await ConfigTlmAsync(checkInterval);
			}
		}

		private void TLM_OnServiceTick(object sender, TwitchLib.Api.Services.Events.OnServiceTickArgs e)
		{
			_discordClient.Logger.LogDebug("[TwitchLiveMonitor] Service tick.");

			// check if twitch channels list updated
			if (DateTime.Compare(File.GetLastWriteTime("twitchchannels.txt"), _lastTwChWrite) <= 0)
				return;
			_twitchChannelList.Clear();
			// update twitch channels
			GetTwitchChannelsAsync();
			_tlm.Stop();
			_tlm.SetChannelsByName(_twitchChannelList);
			_tlm.Start();

			_discordClient.Logger.LogInformation("[TwitchLiveMonitor] Twitch channels updated.");
		}

		private void TLM_OnServiceStarted(object sender, TwitchLib.Api.Services.Events.OnServiceStartedArgs e)
		{
			_discordClient.Logger.LogInformation(
				$"{(_firstLaunch ? $"Monitoring twitch channels: {string.Join(", ", _tlm.ChannelsToMonitor)}." : "[TwitchLiveMonitor] Service resumed.")}");
			if (_firstLaunch)
				_firstLaunch = false;
		}

		private void TLM_OnServiceStopped(object sender, TwitchLib.Api.Services.Events.OnServiceStoppedArgs e)
		{
			_discordClient.Logger.LogWarning("[TwitchLiveMonitor] Service stopped.");
		}

		private void TLM_OnStreamOnline(object sender, OnStreamOnlineArgs e)
		{
			_discordClient.Logger.LogInformation($"[TwitchLiveMonitor] {e.Channel} started streaming.");
			Task.Run(() => SendOnlineNotifAsync(e.Channel, e.Stream.Title));
		}

		private void TLM_OnStreamOffline(object sender, OnStreamOfflineArgs e)
		{
			_discordClient.Logger.LogInformation($"[TwitchLiveMonitor] {e.Channel} stopped streaming.");
			Task.Run(() => SendOfflineNotifAsync(e.Channel));
		}

		private bool GetTwitchChannelsAsync()
		{
			_twitchChannelList = FileHandler.GetChannelListFromFile(_discordClient, "twitchchannels.txt");
			_lastTwChWrite = File.GetLastWriteTime("twitchchannels.txt");            // last write time for twitch channels

			if (_twitchChannelList.Any())
				return true;
			_discordClient.Logger.LogWarning("[TwitchLiveMonitor] " +
				"No channels to monitor. Check twitchchannels.txt file or add channel " +
				"using !admin logging addtwitchchannel <channelname> and restart the bot.");
			return false;
		}

		private async Task SendOnlineNotifAsync(string channel, string title)
		{
			if (_logChannelsList.Any())
			{
				foreach (DiscordChannel logch in _logChannelsList)
				{
					try
					{
						await _discordClient.SendMessageAsync(logch,
							$"@here STREAM STARTED\n{title} on https://www.twitch.tv/{channel}");
					}
					catch (Exception e)
					{
						_discordClient.Logger.LogError($"[TwitchLiveMonitor] Discord message error: {e.Message}");
					}
				}
			}
		}

		private async Task SendOfflineNotifAsync(string channel)
		{
			if (_logChannelsList.Any())
			{
				foreach (DiscordChannel logch in _logChannelsList)
				{
					try
					{
						await _discordClient.SendMessageAsync(logch, $"{channel} stopped streaming.");
					}
					catch (Exception e)
					{
						_discordClient.Logger.LogError($"[TwitchLiveMonitor] Discord message error: {e.Message}");
					}
				}
			}
		}
	}
}
