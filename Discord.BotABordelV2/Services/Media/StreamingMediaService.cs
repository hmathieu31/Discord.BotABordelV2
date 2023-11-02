using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models;

using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;

using Microsoft.Extensions.FileProviders;

namespace Discord.BotABordelV2.Services.Media;

public class StreamingMediaService : MediaService, IMediaService
{
    private readonly IAudioService _audioService;

    public StreamingMediaService(ILogger<StreamingMediaService> logger,
                                 IAudioService audioService)
        : base(logger, audioService)
    {
        _audioService = audioService;
    }

    public override async Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel)
    {
        if (string.IsNullOrEmpty(track))
            throw new ArgumentException($"'{nameof(track)}' cannot be null or empty.", nameof(track));

        var player = await GetStandardPlayerAsync(channel)
            ?? throw new Exceptions.MediaExceptions.NullChannelConnectionException($"Channel connection to '{channel.Name}' failed");

        var foundTrack = await _audioService.Tracks.LoadTrackAsync(track, TrackSearchMode.YouTube);

        if (foundTrack is null)
        {
            _logger.LogDebug("Track not found of name '{track}'", track);
            return new PlayTrackResult(PlayTrackStatus.NoTrackFound);
        }

        var queuePos = await player.PlayAsync(foundTrack);
        _logger.LogInformation("Playing track - {track}", foundTrack.Title);

        return new PlayTrackResult(foundTrack, queuePos);
    }
}