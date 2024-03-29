﻿using Discord.BotABordelV2.Constants;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models.Results;
using Discord.BotABordelV2.Services.Media;
using Discord.Interactions;
using Discord.WebSocket;

using Lavalink4NET.Tracks;

namespace Discord.BotABordelV2.Commands;

[RequireContext(ContextType.Guild)]
public sealed class MusicCommands(StreamingMediaService mediaService,
                     TrollMediaService trollMediaService,
                     IPermissionsService permissionsService,
                     ILogger<MusicCommands> logger) : InteractionModuleBase<SocketInteractionContext>

{
    [SlashCommand("pause", "Pause current track", runMode: RunMode.Async)]
    public async Task Pause()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await mediaService.PauseTrackAsync(channel)).Status switch
        {
            PauseTrackStatus.Paused => MessageResponses.TrackPaused,
            PauseTrackStatus.NothingPlaying => MessageResponses.NothingPlaying,
            PauseTrackStatus.InternalException => MessageResponses.InternalEx,
            PauseTrackStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
            PauseTrackStatus.PlayerNotConnected => MessageResponses.NothingPlaying,
            _ => MessageResponses.InternalEx,
        };

        await RespondAsync(response);
    }

    [SlashCommand("play", "Play a song", runMode: RunMode.Async)]
    [ComponentInteraction("play:*", runMode: RunMode.Async)]
    public async Task Play(
        [Summary("Song", "The song to play")] string song
        )
    {
        try
        {
            // If the command is called from a button, modify the original message
            var ctx = Context.Interaction as SocketMessageComponent;

            if (ctx is not null)
            {
                await UpdateSearchMsg(song, ctx);
            }
            else
            {
                await DeferAsync();
            }

            IVoiceChannel? channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {
                await FollowupAsync(MessageResponses.UserNotConnected);
                return;
            }

            var result = await mediaService.PlayTrackAsync(song, channel);
            if (result.Status is PlayTrackStatus.TrackBanned)
            {
                var res = await trollMediaService.PlayTrackAsync(song, channel);
                await HandlePlayTrackResultAsync(res, result.Track);
                return;
            }

            await HandlePlayTrackResultAsync(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while playing track");
            await FollowupAsync(MessageResponses.InternalEx);
        }
    }

    [SlashCommand("queue", "Display queue of tracks", runMode: RunMode.Async)]
    public async Task Queue()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var result = await mediaService.GetQueueAsync(channel);
        if (result.IsSuccess)
        {
            List<Embed> embeds = [];

            foreach (var queuedTrack in result.QueuedTracks!)
            {
                switch (queuedTrack.Position)
                {
                    case 0:
                        embeds.Add(new EmbedBuilder()
                            .WithTitle($"Now Playing - {queuedTrack.Title}")
                            .WithDescription($"{queuedTrack.Uri}")
                            .WithThumbnailUrl(queuedTrack.ThumbnailUri?.ToString())
                            .Build());

                        break;

                    case 1:
                        embeds.Add(new EmbedBuilder()
                            .WithTitle($"Next up - {queuedTrack.Title}")
                            .WithDescription($"{queuedTrack.Uri}")
                            .WithThumbnailUrl(queuedTrack.ThumbnailUri?.ToString())
                            .Build());

                        break;

                    default:
                        embeds.Add(new EmbedBuilder()
                             .WithTitle($"#{queuedTrack.Position} - {queuedTrack.Title}")
                             .WithDescription($"{queuedTrack.Uri}")
                             .WithThumbnailUrl(queuedTrack.ThumbnailUri?.ToString())
                             .Build());

                        break;
                }
            }

            await RespondAsync(":notes:  Current Queue", embeds: [.. embeds]);
        }
        else
        {
            var failureResponse = result.Status switch
            {
                DisplayQueueStatus.InternalException => MessageResponses.InternalEx,
                DisplayQueueStatus.NothingPlaying => MessageResponses.NothingPlaying,
                DisplayQueueStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
                DisplayQueueStatus.PlayerNotConnected => MessageResponses.NothingPlaying,
                _ => MessageResponses.InternalEx,
            };

            await RespondAsync(failureResponse);
        }
    }

    [SlashCommand("resume", "Resume current paused track", runMode: RunMode.Async)]
    public async Task Resume()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await mediaService.ResumeTrackAsync(channel)).Status switch
        {
            ResumeTrackStatus.Resumed => MessageResponses.TrackResumed,
            ResumeTrackStatus.PlayerNotPaused => MessageResponses.NothingPaused,
            ResumeTrackStatus.InternalException => MessageResponses.InternalEx,
            ResumeTrackStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
            ResumeTrackStatus.PlayerNotConnected => MessageResponses.NothingPaused,
            _ => MessageResponses.InternalEx,
        };
        await RespondAsync(response);
    }

    [SlashCommand("search", "Search for a song", runMode: RunMode.Async)]
    public async Task Search(
        [Summary("Song", "The song to search for")] string song
        )
    {
        await DeferAsync();

        try
        {
            var result = await mediaService.SearchTrackAsync(song);
            if (result.IsSuccess)
            {
                List<Embed> searchEmbeds = [];
                var buttonsBuilder = new ComponentBuilder();

                var i = 0;
                foreach (var track in result.FoundTracks!)
                {
                    i++;

                    searchEmbeds.Add(new EmbedBuilder()
                                    .WithTitle($"{i} - {track.Title}")
                                    .WithDescription(track.Uri?.ToString())
                                    .WithThumbnailUrl(track.ArtworkUri?.ToString())
                                    .Build());

                    buttonsBuilder.WithButton($"{i}", $"play:{track.Uri!}", ButtonStyle.Primary);
                }

                await FollowupAsync(
                    string.Format(MessageResponses.SeachTracksFormat, result.FoundTracks!.Count()),
                    embeds: [.. searchEmbeds],
                    components: buttonsBuilder.Build());
            }
            else
            {
                var error = result.Status switch
                {
                    SearchTrackStatus.NoTrackFound => MessageResponses.NoResults,
                    _ => MessageResponses.InternalEx
                };

                await FollowupAsync(error);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while searching for track");
            await FollowupAsync(MessageResponses.InternalEx);
        }
    }

    [SlashCommand("skip", "Skip the track currently playing", runMode: RunMode.Async)]
    public async Task Skip(
        [Summary("force", "If true, will attempt to force skip")] bool forceSkip = false
        )
    {
        var user = Context.User as IGuildUser;
        var channel = user?.VoiceChannel;

        if (user is null || channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        if (forceSkip && !permissionsService.HasForceSkipPermission(user))
        {
            await RespondAsync(MessageResponses.Unauthorized);
            return;
        }

        SkipTrackResult result = forceSkip
            ? await mediaService.ForceSkipTrackAsync(channel)
            : await mediaService.VoteSkipTrackAsync(channel, Context.User);

        var response = result.Status switch
        {
            SkipTrackStatus.Skipped => string.Format(MessageResponses.SkippedNowPlayingFormat, result.NextTrack!.Title, result.NextTrack.Uri),
            SkipTrackStatus.FinishedQueue => MessageResponses.SkippedFinishedQueue,
            SkipTrackStatus.InternalException => MessageResponses.InternalEx,
            SkipTrackStatus.NothingPlaying => MessageResponses.NothingPlaying,
            SkipTrackStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
            SkipTrackStatus.PlayerNotConnected => MessageResponses.NothingPlaying,
            SkipTrackStatus.VoteSubmitted => string.Format(MessageResponses.VotedSkipFormat, result.VotesInfo!.Value.Percentage),
            SkipTrackStatus.AlreadySubmitted => MessageResponses.AlreadyVotedSkip,
            _ => MessageResponses.InternalEx,
        };
        await RespondAsync(response);
    }

    [SlashCommand("stop", "Stop the player and clears the queue", runMode: RunMode.Async)]
    public async Task Stop()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await mediaService.StopPlayerAsync(channel)).Status switch
        {
            StopPlayerStatus.Stopped => MessageResponses.PlayerStopped,
            StopPlayerStatus.InternalException => MessageResponses.InternalEx,
            StopPlayerStatus.NothingPlaying => MessageResponses.NothingPlaying,
            StopPlayerStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
            StopPlayerStatus.PlayerNotConnected => MessageResponses.NothingPlaying,
            _ => MessageResponses.InternalEx,
        };
        await RespondAsync(response);
    }

    private static async Task UpdateSearchMsg(string selectedSong, SocketMessageComponent ctx)
    {
        var searchMsgComponents = ctx.Message.Components;
        var newBtnsBuilder = new ComponentBuilder();
        foreach (var btn in from actRow in searchMsgComponents
                            from comp in actRow.Components
                            where comp is ButtonComponent
                            select comp as ButtonComponent)
        {
            if (btn.CustomId == $"play:{selectedSong}")
                newBtnsBuilder.WithButton($"{btn.Label}     ▶️", btn.CustomId, ButtonStyle.Success, disabled: true);
            else
                newBtnsBuilder.WithButton(btn.Label, btn.CustomId, btn.Style, disabled: true);
        }

        await ctx.UpdateAsync(x => x.Components = newBtnsBuilder.Build());
    }

    private async Task HandlePlayTrackResultAsync(PlayTrackResult result, LavalinkTrack? playedTrackOverride = default)
    {
        if (result.IsSuccess)
        {
            var track = playedTrackOverride ?? result.Track;
            var response = result.Status switch
            {
                PlayTrackStatus.Playing => string.Format(MessageResponses.PlayingTrackFormat, track.Title, track.Uri),
                PlayTrackStatus.Queued => string.Format(MessageResponses.QueuedTrackFormat, track.Title, track.Uri),
                _ => throw new NotImplementedException(),
            };

            await FollowupAsync(response);
        }
        else
        {
            var error = result.Status switch
            {
                PlayTrackStatus.NoTrackFound => MessageResponses.NoResults,
                PlayTrackStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
                _ => MessageResponses.InternalEx
            };

            await FollowupAsync(error);
        }
    }
}