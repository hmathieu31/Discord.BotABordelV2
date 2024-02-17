using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models.Results;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services.Media;

public class StreamingMediaService(ILogger<StreamingMediaService> logger,
                             IAudioService audioService,
                             IOptionsMonitor<DiscordBotOptions> botOptions) : MediaService(logger, audioService, botOptions), IMediaService
{
    public override async Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel)
    {
        if (string.IsNullOrEmpty(track))
            throw new ArgumentException($"'{nameof(track)}' cannot be null or empty.", nameof(track));

        var player = await GetPlayerAsync(channel)
            ?? throw new Exceptions.MediaExceptions.NullChannelConnectionException($"Channel connection to '{channel.Name}' failed");

        var foundTrack = await AudioService.Tracks.LoadTrackAsync(track, TrackSearchMode.YouTube);

        if (foundTrack is null)
        {
            logger.LogDebug("Track not found of name '{track}'", track);
            return new PlayTrackResult(PlayTrackStatus.NoTrackFound);
        }

        var queuePos = await player.PlayAsync(foundTrack);
        logger.LogInformation("Playing track - {track}", foundTrack.Title);

        return new PlayTrackResult(foundTrack, queuePos);
    }
}