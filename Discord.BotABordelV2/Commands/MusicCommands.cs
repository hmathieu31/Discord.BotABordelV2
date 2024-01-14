using Discord.BotABordelV2.Constants;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models;
using Discord.BotABordelV2.Services.Media;
using Discord.Interactions;

namespace Discord.BotABordelV2.Commands;

[RequireContext(ContextType.Guild)]
public sealed class MusicCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly StreamingMediaService _mediaService;
    private readonly IPermissionsService _permissionsService;

    public MusicCommands(StreamingMediaService mediaService,
                         IPermissionsService permissionsService)
    {
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        _permissionsService = permissionsService;
    }

    [SlashCommand("pause", "Pause current track", runMode: RunMode.Async)]
    public async Task Pause()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await _mediaService.PauseTrackAsync(channel)).Status switch
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
    public async Task Play(
        [Summary("Song", "The song to play")] string song
        )
    {
        await DeferAsync();

        IVoiceChannel? channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel is null)
        {
            await FollowupAsync(MessageResponses.UserNotConnected);
            return;
        }

        var result = await _mediaService.PlayTrackAsync(song, channel);
        if (result.IsSuccess)
        {
            var response = result.Status switch
            {
                Models.PlayTrackStatus.Playing => string.Format(MessageResponses.PlayingTrackFormat, result.Track!.Title, result.Track.Uri),
                Models.PlayTrackStatus.Queued => string.Format(MessageResponses.QueuedTrackFormat, result.Track!.Title, result.Track.Uri),
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

    [SlashCommand("resume", "Resume current paused track", runMode: RunMode.Async)]
    public async Task Resume()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await _mediaService.ResumeTrackAsync(channel)).Status switch
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

        if (forceSkip && !_permissionsService.HasForceSkipPermission(user))
        {
            await RespondAsync(MessageResponses.Unauthorized);
            return;
        }

        SkipTrackResult result = await _mediaService.SkipTrackAsync(channel, Context.User, false);
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

        var response = (await _mediaService.StopPlayerAsync(channel)).Status switch
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
}