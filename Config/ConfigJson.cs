using System.Text.Json.Serialization;

namespace leetbot_night.Config
{
	public class ConfigJson
	{
		[JsonPropertyName("token")]
		public string Token { get; set; }

		[JsonPropertyName("prefix")]
		public string CommandPrefix { get; set; }

		[JsonPropertyName("enable_dms")]
		public bool DmsEnabled { get; set; }

		[JsonPropertyName("enable_mention")]
		public bool MentionEnabled { get; set; }

		[JsonPropertyName("case_sensitive")]
		public bool CaseSensitive { get; set; }

		[JsonPropertyName("actiontimeout")]
		public int ActTimeout { get; set; }

		[JsonPropertyName("twitchAPIclid")]
		public string TwitchApiClid { get; set; }

		[JsonPropertyName("twitchAPIsecret")]
		public string TwitchApiSecret { get; set; }

		[JsonPropertyName("youtubeAPIkey")]
		public string YoutubeApiKey { get; set; }

		[JsonPropertyName("AesKey")]
		public string AesKey { get; set; }

		[JsonPropertyName("AesIv")]
		public string AesIv { get; set; }

		[JsonPropertyName("RsaPublicKey")]
		public string RsaPublicKey { get; set; }

		[JsonPropertyName("RsaPrivateKey")]
		public string RsaPrivateKey { get; set; }

		public ConfigJson()
		{
			Token = "null";
			CommandPrefix = "your command prefix";
			DmsEnabled = true;
			MentionEnabled = false;
			CaseSensitive = false;
			ActTimeout = 2;
			TwitchApiClid = "null";
			TwitchApiSecret = "null";
			YoutubeApiKey = "null";
			AesKey = "null";
			AesIv = "null";
			RsaPublicKey = "null";
			RsaPrivateKey = "null";
		}
	}
}
