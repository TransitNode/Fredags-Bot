using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Fredags_Bot.scr.Core.Commands
{
    public class Commands : BaseCommandModule
    {
        private async Task<LavalinkNodeConnection> GetConnectedNode(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return null;
            }

            return lava.ConnectedNodes.Values.First();
        }

        private async Task<LavalinkGuildConnection> GetGuildConnection(CommandContext ctx, LavalinkNodeConnection node)
        {
            var member = ctx.Member;
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return null;
            }

            return node.GetGuildConnection(member.VoiceState.Guild);
        }

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var channel = ctx.Member.VoiceState.Channel;
            await node.ConnectAsync(channel);

            await ctx.RespondAsync($"Joined {channel.Name}!");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var conn = await GetGuildConnection(ctx, node);
            if (conn == null) return;

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {ctx.Member.VoiceState.Channel.Name}!");
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string input)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var conn = await GetGuildConnection(ctx, node);

            var userVoiceChannel = ctx.Member.VoiceState.Channel;
            if (userVoiceChannel == null)
            {
                await ctx.RespondAsync("You must be in a voice channel to use this command!");
                return;
            }

            var botVoiceChannel = conn?.Channel;

            if (botVoiceChannel == null || (botVoiceChannel != userVoiceChannel && conn.CurrentState.CurrentTrack == null))
            {
                conn = await node.ConnectAsync(userVoiceChannel);
            }
            else if (botVoiceChannel != userVoiceChannel)
            {
                await ctx.RespondAsync("I'm already playing in another channel!");
                return;
            }

            if (ctx.Channel.Id != 1094295537253109770) // Replace with the specific channel ID where the command is allowed
            {
                await ctx.RespondAsync("This command can only be used in the designated bot channel.");
                return;
            }

            if (Uri.TryCreate(input, UriKind.Absolute, out var url) && (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps))
            {
                var track = await node.Rest.GetTracksAsync(url.ToString());
                var selectedTrack = track.Tracks.First();

                if (track.LoadResultType == LavalinkLoadResultType.LoadFailed || track.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    await ctx.RespondAsync($"Track loading failed for {input}.");
                    return;
                }

                await conn.PlayAsync(selectedTrack);

                var embed = new DiscordEmbedBuilder
                {
                    Title = "🎶 Requested Song:",
                    Description = $"[{selectedTrack.Title}]({selectedTrack.Uri})",
                    Color = new DiscordColor("#1DB954"),
                    Timestamp = DateTimeOffset.UtcNow
                };

                embed.AddField("Length", selectedTrack.Length.ToString(@"mm\:ss"), false);
                embed.AddField("URL", selectedTrack.Uri.ToString(), false);
                embed.AddField("Author", selectedTrack.Author, false);
                embed.AddField("Requested By", ctx.Member.Mention, false);

                await ctx.Message.DeleteAsync();

                var embedMessage = await ctx.RespondAsync(embed: embed);

                AsyncEventHandler<LavalinkGuildConnection, TrackFinishEventArgs> handler = null;
                handler = async (_, args) =>
                {
                    if (args.Track == selectedTrack)
                    {
                        await embedMessage.DeleteAsync();
                        conn.PlaybackFinished -= handler; // Unregister the handler once the track finishes playing
                    }
                };

                conn.PlaybackFinished += handler;
            }
            else
            {
                await ctx.RespondAsync("Invalid input. Please provide a valid URL.");
            }
        }


        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var conn = await GetGuildConnection(ctx, node);
            if (conn == null || conn.CurrentState.CurrentTrack == null) return;

            await conn.PauseAsync();
            await ctx.RespondAsync("Paused playback.");
        }

        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var conn = await GetGuildConnection(ctx, node);
            if (conn == null || conn.CurrentState.CurrentTrack == null) return;

            await conn.StopAsync();
            await ctx.RespondAsync("Stopped playback.");
        }

        [Command("resume")]
        public async Task Resume(CommandContext ctx)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var conn = await GetGuildConnection(ctx, node);
            if (conn == null || conn.CurrentState.CurrentTrack == null) return;

            await conn.ResumeAsync();
            await ctx.RespondAsync("Resumed playback.");
        }

        [Command("jump")]
        public async Task Jump(CommandContext ctx, string time)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var conn = await GetGuildConnection(ctx, node);
            if (conn == null || conn.CurrentState.CurrentTrack == null) return;

            if (
                TimeSpan.TryParseExact(time, @"h\:m\:s", CultureInfo.InvariantCulture, out var jumpTime)
            )
            {
                await conn.SeekAsync(jumpTime);
                await ctx.RespondAsync($"Jumped to {jumpTime} in the track.");
            }
            else
            {
                await ctx.RespondAsync("Invalid time format. Use format like '0:0:30' for 30 seconds.");
            }
        }

        [Command("volume")]
        public async Task SetVolume(CommandContext ctx, int volume)
        {
            var node = await GetConnectedNode(ctx);
            if (node == null) return;

            var conn = await GetGuildConnection(ctx, node);
            if (conn == null) return;

            if (volume < 0 || volume > 100)
            {
                await ctx.RespondAsync("Volume must be between 0 and 100.");
                return;
            }

            await conn.SetVolumeAsync(volume);
            await ctx.RespondAsync($"Volume set to {volume}%.");
        }

        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            var embed = new DSharpPlus.Entities.DiscordEmbedBuilder
            {
                Title = "Available Commands",
                Description = "Here are the commands you can use:",
                Color = DSharpPlus.Entities.DiscordColor.CornflowerBlue
            };

            embed.AddField("!join", "Joins a voice channel.");
            embed.AddField("!leave", "Leaves the current voice channel.");
            embed.AddField("!play [URL]", "Plays a track from the given URL.");
            embed.AddField("!pause", "Pauses playback.");
            embed.AddField("!stop", "Stops playback.");
            embed.AddField("!resume", "Resumes playback.");
            embed.AddField("!jump [time]", "Jumps to a specific time in the track. Format: '0:0:0' (hours:minutes:seconds).");
            embed.AddField("!volume [0-100]", "Sets the volume to a specific level.");

            await ctx.RespondAsync(embed: embed);
        }
    }
}