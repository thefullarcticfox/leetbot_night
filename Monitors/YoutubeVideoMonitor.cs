using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using leetbot_night.Services;
using Microsoft.Extensions.Logging;

namespace leetbot_night.Monitors
{
	public class YoutubeVideoMonitor
	{
		private DiscordClient			_discordClient;
		private List<DiscordChannel>	_logChannelsList;
		private YouTubeService			_yts;
		private bool					_isRunning;
		private List<string>			_youtubeChannelList;
		private DateTime				_tempDateTime;
		private string					_ytApiKey;

		public virtual void RunService(DiscordClient client, string apiKey,
			List<DiscordChannel> discordChannels, int checkInterval)
		{
			_discordClient = client;
			_logChannelsList = discordChannels;      // log channels list
			// get youtube channels to list
			_youtubeChannelList = FileHandler.GetChannelListFromFile(_discordClient, "youtubechannels.txt");
			_ytApiKey = apiKey;
			// check if not empty
			if (_youtubeChannelList.Any())
				Task.Run(() => RunYoutubeService(checkInterval));
			else
			{
				_discordClient.Logger.LogWarning("[YoutubeService] No channels to monitor. Check youtubechannels.txt file.");
			}
		}

		public virtual bool MonitorRunning()
		{
			return _isRunning;
		}

		public virtual void UpdateLogChannels(List<DiscordChannel> discordChannels)
		{
			_logChannelsList.Clear();
			_logChannelsList = discordChannels;
		}

		private async Task RunYoutubeService(int checkInterval)
		{
			_tempDateTime = DateTime.Now;              // saving time of launch
			_yts = new YouTubeService(new BaseClientService.Initializer
			{
				ApiKey = _ytApiKey,
				ApplicationName = "leetbot"
			});

			PlaylistItemsResource.ListRequest playlistItemsListRequest = _yts.PlaylistItems.List("contentDetails");
			playlistItemsListRequest.MaxResults = 5;                // last 5 videos
			playlistItemsListRequest.Fields = "items(contentDetails(videoId,videoPublishedAt))";

			_discordClient.Logger.LogInformation("[YoutubeService] Service started.");

			_isRunning = true;

			while (true)
			{
				foreach (string uploadsId in _youtubeChannelList)
				{
					playlistItemsListRequest.PlaylistId = uploadsId;
					PlaylistItemListResponse playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

					foreach (PlaylistItem playlistItem in playlistItemsListResponse.Items)
					{
						if (playlistItem.ContentDetails.VideoPublishedAt == null ||
							playlistItem.ContentDetails.VideoPublishedAt.GetValueOrDefault().CompareTo(_tempDateTime) <= 0)
							continue;

						_discordClient.Logger.LogInformation("[YoutubeService] " +
							"New video uploaded: https://www.youtube.com/watch?v=" +
							$"{playlistItem.ContentDetails.VideoId}");

						foreach (DiscordChannel logChannel in _logChannelsList)
						{
							try
							{
								await _discordClient.SendMessageAsync(logChannel,
									"@everyone NEW VIDEO\nhttps://www.youtube.com/watch?v=" +
									$"{playlistItem.ContentDetails.VideoId}");
							}
							catch (Exception e)
							{
								_discordClient.Logger.LogError($"[YoutubeVideoMonitor] Discord message error: {e.Message}");
							}
						}
						_tempDateTime = DateTime.Now;
					}
				}
				await Task.Delay(checkInterval * 60 * 1000);    // once in checkInterval minutes
				_discordClient.Logger.LogDebug("[YoutubeVideoMonitor] Service tick.");
			}
		}
	}
}
