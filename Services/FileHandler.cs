#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using leetbot_night.Config;

namespace leetbot_night.Services
{
    public sealed class FileHandler
    {
        private static async Task CreateWriteTxtFile(string filename, string textinput)
        {
            try
            {
                StreamWriter sw = File.CreateText(filename);
                await sw.WriteAsync(textinput);
                sw.Close();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Can't create {filename} file.");
                Console.WriteLine("Unauthorized Access Error: " + e.Message);
                Console.ResetColor();
            }
        }

        public static async Task<string> ReadJsonConfig()
        {
            string result;
            try
            {
                await using (FileStream fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    result = await sr.ReadToEndAsync();
                if (result.Length == 0)
                    throw new FileNotFoundException();
            }
            catch (FileNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Config file not found or it's empty. Creating config.json ...");
                Console.ResetColor();
                var defaultJsonConfig = new ConfigJson();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string defaultConfig = JsonSerializer.Serialize(defaultJsonConfig, options);
                await CreateWriteTxtFile("config.json", defaultConfig);
                result = "default";
            }

            return result;
        }

        public static List<string> GetChannelListFromFile(DiscordClient discord, string file)
        {
            var channelList = new List<string>();

            try
            {
                channelList = File.ReadAllLines(file).ToList();
            }
            catch (FileNotFoundException)
            {
                discord.Logger.LogWarning("[FileHandler] " +
                    $"{file} not found. Creating one...");
                CreateWriteTxtFile(file, "").Wait();
                discord.Logger.LogInformation("[FileHandler] " +
                    $"Created {file} file.");
            }
            return channelList;
        }

        public static async Task<List<DiscordChannel>> GetLogChannelsAsync(DiscordClient discord)
        {
            List<string> channels = GetChannelListFromFile(discord, "logchannels.txt");
            var logChannelsList = new List<DiscordChannel>();

            foreach (var logchchain in channels.Select(ch => ch.Split(' ')))
            {
                try
                {
                    DiscordChannel? channel = await discord.GetChannelAsync(Convert.ToUInt64(logchchain[1]));
                    logChannelsList.Add(channel);
                }
                catch (Exception)
                {
                    discord.Logger.LogWarning("[FileHandler] " +
                        $"Channel {logchchain[1]} not found. Ignoring.");
                }
            }
            if (!logChannelsList.Any())
                discord.Logger.LogWarning("[FileHandler] " +
                    "No log channels. Check logchannels.txt file.");
            return logChannelsList;
        }

        public static string GetRandomFile(string path, params string[] extensions)
        {
            string file = "File not found.";
            if (string.IsNullOrEmpty(path))
                return file;
            var dir = new DirectoryInfo(path);
            IEnumerable<FileInfo> rgFiles = dir.GetFiles("*.*")
                .Where(f => extensions.Contains(f.Extension.ToLower()));
            var r = new Random();
            List<FileInfo> fileInfos = rgFiles.ToList();
            if (fileInfos.Any())
                file = fileInfos.ElementAt(r.Next(0, fileInfos.Count)).FullName;
            return file;
        }

        public static int GetFilesCount(string path, params string[] extensions)
        {
            var imgcount = 0;
            if (string.IsNullOrEmpty(path))
                return imgcount;
            var dir = new DirectoryInfo(path);
            IEnumerable<FileInfo> rgFiles = dir.GetFiles("*.*")
                .Where(f => extensions.Contains(f.Extension.ToLower()));
            imgcount = rgFiles.Count();
            return imgcount;
        }

        public static Task DownloadUrlFile(string url, string path, DiscordClient discord, string filename = "")
        {
            var uri = new Uri(url);
            if (filename.Length == 0)
                filename = Path.GetFileName(uri.LocalPath);
            if (!Directory.Exists(path))
            {
                discord.Logger.LogWarning("[FileHandler] " +
                    $"Directory {path} not found. Creating...");
                DirectoryInfo dir = Directory.CreateDirectory(path);
                if (dir.Exists)
                    discord.Logger.LogInformation("[FileHandler] " +
                        $"Successfully created {dir.FullName} directory.");
            }

            using var wc = new WebClient();
            wc.DownloadFileCompleted += (_, e) =>
                Wc_DownloadFileCompleted(e, filename, path, discord);
            wc.DownloadFileTaskAsync(uri, path + filename);
            return Task.CompletedTask;
        }

        private static void Wc_DownloadFileCompleted(AsyncCompletedEventArgs e, string filename, string path, DiscordClient discord)
        {
            if (e.Error != null)
            {
                string errormsg = e.Error.Message;
                Exception? einner = e.Error.InnerException;
                while (einner != null)
                {
                    errormsg += $" -> {einner.Message}";
                    einner = einner.InnerException;
                }
                discord.Logger.LogWarning("[FileHandler] " +
                    $"Error downloading {filename} to {path}. {errormsg}");
            }
            else
                discord.Logger.LogInformation("[FileHandler] " +
                    $"Successfully downloaded {filename} to {path}.");
        }
    }
}
