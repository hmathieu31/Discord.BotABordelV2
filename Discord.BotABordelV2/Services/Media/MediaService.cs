using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models;
using Discord.Interactions;

using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Preconditions;
using Lavalink4NET.Players.Vote;

using Microsoft.Extensions.Options;

using System.Collections.Immutable;

namespace Discord.BotABordelV2.Services.Media;

public abstract class MediaService : IMediaService
{
    protected readonly ILogger _logger;
    private readonly IAudioService _audioService;

    protected MediaService(ILogger logger,
                           IAudioService audioService)
    {
        _logger = logger;
        _audioService = audioService;
    }

    public abstract Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel);

    public async Task<StopPlayerResult> StopPlayerAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetStandardPlayerAsync(channel, false);

            if (player is null)
                return new StopPlayerResult(StopPlayerStatus.PlayerNotConnected);

            if (player.CurrentTrack is null)
                return new StopPlayerResult(StopPlayerStatus.NothingPlaying);

            await player.StopAsync();
            return new StopPlayerResult(StopPlayerStatus.Stopped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while trying to stop player");
            return new StopPlayerResult(StopPlayerStatus.InternalException);
        }
    }

    public async Task<PauseTrackResult> PauseTrackAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetStandardPlayerAsync(channel, false);

            if (player is null)
                return new PauseTrackResult(PauseTrackStatus.PlayerNotConnected);

            if (player.CurrentTrack is null)
                return new PauseTrackResult(PauseTrackStatus.NothingPlaying);

            await player.PauseAsync();
            return new PauseTrackResult(PauseTrackStatus.Paused);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while trying to Pause playing track");
            return new PauseTrackResult(PauseTrackStatus.InternalException);
        }
    }

    public async Task<ResumeTrackResult> ResumeTrackAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetStandardPlayerAsync(channel, false);

            if (player is null)
                return new ResumeTrackResult(ResumeTrackStatus.PlayerNotConnected);

            if (player.State is not PlayerState.Paused)
                return new ResumeTrackResult(ResumeTrackStatus.PlayerNotPaused);

            await player.ResumeAsync();
            return new ResumeTrackResult(ResumeTrackStatus.Resumed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while trying to Resume playing");
            return new ResumeTrackResult(ResumeTrackStatus.InternalException);
        }
    }

    public async Task<SkipTrackResult> SkipTrackAsync(IVoiceChannel channel)
    {
        try
        {
            var player = await GetStandardPlayerAsync(channel, false);

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
            _logger.LogError(ex, "Exception while trying to Skip");
            return new SkipTrackResult(SkipTrackStatus.InternalException);
        }
    }

    protected async Task<IVoteLavalinkPlayer?> GetStandardPlayerAsync(IVoiceChannel channel, bool connectToChannel = true)
    {
        var playerOptions = new VoteLavalinkPlayerOptions
        {
            SkipThreshold = 0.66
        };

        var result = await _audioService.Players.RetrieveAsync(
            channel.Guild.Id,
            channel.Id,
            PlayerFactory.Vote,
            Options.Create(playerOptions),
            new PlayerRetrieveOptions(connectToChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None)
            );

        if (!result.IsSuccess)
        {
            _logger.LogError("Player retrieval failed with status {status}", result.Status);
            return null;
        }

        return result.Player;
    }

    protected async Task<ILavalinkPlayer?> GetGrandEntrancePlayerAsync(IVoiceChannel channel)
    {
        var playerOptions = new LavalinkPlayerOptions
        {
            DisconnectOnStop = true,
        };

        ImmutableArray<IPlayerPrecondition> preconditions = ImmutableArray.Create(PlayerPrecondition.NotPlaying);

        var result = await _audioService.Players.RetrieveAsync(
            channel.Guild.Id,
            channel.Id,
            PlayerFactory.Default,
            Options.Create(playerOptions),
            new PlayerRetrieveOptions(
                PlayerChannelBehavior.Join,
                Preconditions: preconditions
                )
            );

        if (!result.IsSuccess)
        {
            if (result.Status is PlayerRetrieveStatus.PreconditionFailed
                && result.Precondition == PlayerPrecondition.NotPlaying)
            {
                _logger.LogInformation("The player is already playing a track");
                return null;
            }

            _logger.LogError("Player retrieval failed with status {status}", result.Status);
            return null;
        }

        return result.Player;
    }
}