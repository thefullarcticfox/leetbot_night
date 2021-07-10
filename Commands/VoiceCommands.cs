using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using leetbot_night.Services;

namespace leetbot_night.Commands
{
    [Group("voice"), Hidden, RequireOwner, RequireGuild,
     Description("Voice commands.")]
    public class VoiceCommands : BaseCommandModule
    {
        private VoiceNextExtension  _vnext;

        private async Task<VoiceNextConnection> GetVNextConnection(CommandContext ctx)
        {
            if (ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync(":x: This command is only for server use.");
                return null;
            }
            VoiceNextConnection vnc = null;
            if (_vnext != null)
                vnc = _vnext.GetConnection(ctx.Guild);
            if (vnc == null)
                await ctx.Message.RespondAsync(":x: Voice is not connected in this guild.");
            return vnc;
        }

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Check `!help voice` for command usage.");
        }

        [Command("join"),
         Description("joins a voice channel.")]
        public async Task Join(CommandContext ctx,
            DiscordChannel chn = null)
        {
            _vnext = ctx.Client.GetVoiceNext();
            if (_vnext == null)
            {
                await ctx.RespondAsync(":x: VNext is not enabled or configured.");
                return;
            }
            VoiceNextConnection vnc = _vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Already connected.");
                return;
            }
            DiscordVoiceState vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }
            if (chn == null)
                chn = vstat.Channel;
            await chn.ConnectAsync();
            await ctx.RespondAsync($"Connected to `{chn.Name}`");
        }

        [Command("leave"),
         Description("leaves voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            VoiceNextExtension voice = ctx.Client.GetVoiceNext();
            if (voice == null)
            {
                await ctx.Message.RespondAsync("Voice is not activated").ConfigureAwait(false);
                return;
            }

            VoiceNextConnection vnc = await GetVNextConnection(ctx);
            if (vnc == null)
                return;
            if (vnc.IsPlaying)
                vnc.Pause();
            vnc.Disconnect();
            await ctx.RespondAsync("Disconnected");
        }

        [Command("play"),
         Description("plays an audio file.")]
        public async Task Play(CommandContext ctx,
            [Description("full path to the file to play."), RemainingText] string filename)
        {
            VoiceNextConnection vnc = await GetVNextConnection(ctx);
            if (vnc == null)
                return;
            if (filename == null)
            {
                await ctx.RespondAsync("No file specified.");
                return;
            }
            if (!File.Exists(filename))
            {
                await ctx.RespondAsync($"File `{filename}` does not exist.");
                return;
            }
            while (vnc.IsPlaying)
            {
                await ctx.Message.RespondAsync("Waiting for audio to end.");
                await vnc.WaitForPlaybackFinishAsync();
            }
            await ctx.Message.RespondAsync($"Playing `{filename}`");
            await vnc.SendSpeakingAsync();
            try
            {
                /* borrowed from
                 * https://github.com/RogueException/Discord.Net/blob/5ade1e387bb8ea808a9d858328e2d3db23fe0663/docs/guides/voice/samples/audio_create_ffmpeg.cs
                 */
                var ffmpegInf = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{filename}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                Process ffmpeg = Process.Start(ffmpegInf);
                if (ffmpeg != null)
                {
                    Stream ffout = ffmpeg.StandardOutput.BaseStream;

                    VoiceTransmitSink transmit = vnc.GetTransmitSink();
                    await ffout.CopyToAsync(transmit);
                    await transmit.FlushAsync();

                    await vnc.WaitForPlaybackFinishAsync();
                    ffout.Close();
                }
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync($"An exception occured during playback: `{ex.GetType()}: {ex.Message}`");
            }
        }

        [Command("playrandom"),
         Description("plays random track from music folder.")]
        public async Task PlayRandom(CommandContext ctx,
            [Description("full path to the folder."), RemainingText] string folder)
        {
            if (folder == null)
            {
                await ctx.RespondAsync("No folder specified.");
                return;
            }
            if (!Directory.Exists(folder))
            {
                await ctx.RespondAsync($"Folder `{folder}` does not exist.");
                return;
            }

            string rndfile = FileHandler.GetRandomFile(folder, ".mp3");
            if (rndfile == "File not found.")
                await ctx.RespondAsync("Folder does not contain mp3 files.");
            else
                await Play(ctx, rndfile);
        }

        [Command("volume"),
         Description("change volume.")]
        public async Task VolumeAsync(CommandContext ctx,
            [Description("volume in percents: from 0 to 250 inclusive.")] int vol)
        {
            VoiceNextConnection vnc = await GetVNextConnection(ctx);
            if (vnc == null)
                return;
            if (vol < 0 || vol > 250)
            {
                await ctx.RespondAsync(":x: Volume needs to be between 0 and 250% inclusive.");
                return;
            }
            VoiceTransmitSink transmitStream = vnc.GetTransmitSink();
            transmitStream.VolumeModifier = vol * 0.01;
            await ctx.RespondAsync($"Volume set to {vol}%");
        }

        [Command("pause"),
         Description("pauses playback.")]
        public async Task PausePlayback(CommandContext ctx)
        {
            VoiceNextConnection vnc = await GetVNextConnection(ctx);
            if (vnc == null)
                return;
            if (vnc.IsPlaying)
                vnc.Pause();
        }

        [Command("resume"),
         Description("continues playback.")]
        public async Task ResumePlayback(CommandContext ctx)
        {
            VoiceNextConnection vnc = await GetVNextConnection(ctx);
            if (vnc == null)
                return;
            if (!vnc.IsPlaying)
                await vnc.ResumeAsync();
        }
    }
}
