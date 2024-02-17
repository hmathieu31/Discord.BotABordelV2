using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models.Results;
using Discord.BotABordelV2.Models.Tracks;
using Discord.BotABordelV2.Players;

using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Rest.Entities.Tracks;

using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services.Media;

public abstract class MediaService(ILogger logger,
                                IAudioService audioService,
                                IOptionsMonitor<DiscordBotOptions> botOptions) : IMediaService
{
    protected IAudioService AudioService => audioService;

    public async Task<SkipTrackResult> ForceSkipTrackAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetPlayerAsync(channel, false);

            if (player is null)
                return new SkipTrackResult(SkipTrackStatus.PlayerNotConnected);

            if (player.CurrentTrack is null)
                return new SkipTrackResult(SkipTrackStatus.NothingPlaying);

            await player.SkipAsync();

            var track = player.CurrentTrack;

            if (track is null)
                return new SkipTrackResult(SkipTrackStatus.FinishedQueue);

            return new SkipTrackResult(track);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while trying to Skip");
            return new SkipTrackResult(SkipTrackStatus.InternalException);
        }
    }

    public async Task<GetQueueResult> GetQueueAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetPlayerAsync(channel, false);

            if (player is null)
                return new GetQueueResult(DisplayQueueStatus.PlayerNotConnected);

            var queue = player.Queue;
            if (player.CurrentTrack is null && queue.IsEmpty)
                return new GetQueueResult(DisplayQueueStatus.NothingPlaying);

            var queuedTracks = queue.Select((t, i) => new TrackQueueItem(t.Identifier,
                                                                         t.Track!.Title,
                                                                         t.Track.Uri,
                                                                         t.Track.ArtworkUri,
                                                                         i + 1))
                                                     .ToList();

            queuedTracks.Add(new TrackQueueItem(
                player.CurrentTrack!.Identifier,
                player.CurrentTrack.Title,
                player.CurrentTrack.Uri,
                player.CurrentTrack.ArtworkUri,
                0));

            queuedTracks = [.. queuedTracks.OrderBy(t => t.Position)];

            return new GetQueueResult(queuedTracks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while trying to display queue");
            return new GetQueueResult(DisplayQueueStatus.InternalException);
        }
    }

    public async Task<PauseTrackResult> PauseTrackAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetPlayerAsync(channel, false);

            if (player is null)
                return new PauseTrackResult(PauseTrackStatus.PlayerNotConnected);

            if (player.CurrentTrack is null)
                return new PauseTrackResult(PauseTrackStatus.NothingPlaying);

            await player.PauseAsync();
            return new PauseTrackResult(PauseTrackStatus.Paused);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while trying to Pause playing track");
            return new PauseTrackResult(PauseTrackStatus.InternalException);
        }
    }

    public abstract Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel);

    public async Task<ResumeTrackResult> ResumeTrackAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetPlayerAsync(channel, false);

            if (player is null)
                return new ResumeTrackResult(ResumeTrackStatus.PlayerNotConnected);

            if (player.State is not PlayerState.Paused)
                return new ResumeTrackResult(ResumeTrackStatus.PlayerNotPaused);

            await player.ResumeAsync();
            return new ResumeTrackResult(ResumeTrackStatus.Resumed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while trying to Resume playing");
            return new ResumeTrackResult(ResumeTrackStatus.InternalException);
        }
    }

    public async Task<SearchTrackResult> SearchTrackAsync(string trackTitle)
    {
        var returedTracksNb = botOptions.CurrentValue.TracksReturnedPerSearch;

        try
        {
            var searchResult = await audioService.Tracks
                                .LoadTracksAsync(trackTitle, TrackSearchMode.YouTube);

            if (!searchResult.IsSuccess)
                return new SearchTrackResult(SearchTrackStatus.NoTrackFound);

            return new SearchTrackResult(searchResult.Tracks.Take(returedTracksNb));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while trying to search for track");
            return new SearchTrackResult(SearchTrackStatus.InternalException);
        }
    }

    public async Task<StopPlayerResult> StopPlayerAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetPlayerAsync(channel, false);

            if (player is null)
                return new StopPlayerResult(StopPlayerStatus.PlayerNotConnected);

            if (player.CurrentTrack is null)
                return new StopPlayerResult(StopPlayerStatus.NothingPlaying);

            await player.StopAsync();
            return new StopPlayerResult(StopPlayerStatus.Stopped);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while trying to stop player");
            return new StopPlayerResult(StopPlayerStatus.InternalException);
        }
    }

    public async Task<SkipTrackResult> VoteSkipTrackAsync(IVoiceChannel channel, IUser user)
    {
        try
        {
            var player = await GetPlayerAsync(channel, false);

            if (player is null)
                return new SkipTrackResult(SkipTrackStatus.PlayerNotConnected);

            if (player.CurrentTrack is null)
                return new SkipTrackResult(SkipTrackStatus.NothingPlaying);

            var voteResult = await player.VoteAsync(user.Id, new UserVoteOptions());

            switch (voteResult)
            {
                case UserVoteResult.Submitted:
                    return new SkipTrackResult(await player.GetVotesAsync(), VoteSubmitStatus.VoteSubmitted);

                case UserVoteResult.AlreadySubmitted:
                    return new SkipTrackResult(await player.GetVotesAsync(), VoteSubmitStatus.AlreadySubmitted);

                case UserVoteResult.UserNotInChannel:
                    return new SkipTrackResult(SkipTrackStatus.UserNotInVoiceChannel);
            }

            var track = player.CurrentTrack;

            if (track is null)
                return new SkipTrackResult(SkipTrackStatus.FinishedQueue);

            return new SkipTrackResult(track);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while trying to Skip");
            return new SkipTrackResult(SkipTrackStatus.InternalException);
        }
    }

    protected async Task<BotaPlayer?> GetPlayerAsync(IVoiceChannel channel, bool connectToChannel = true)
    {
        var playerOptions = new VoteLavalinkPlayerOptions
        {
            SkipThreshold = 0.66
        };

        var result = await audioService.Players.RetrieveAsync<BotaPlayer, VoteLavalinkPlayerOptions>(
            channel.Guild.Id,
            channel.Id,
            BotaPlayer.CreatePlayerAsync,
            Options.Create(playerOptions),
            new PlayerRetrieveOptions(connectToChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None)
            );

        if (!result.IsSuccess)
        {
            logger.LogError("Player retrieval failed with status {status}", result.Status);
            return null;
        }

        return result.Player;
    }
}