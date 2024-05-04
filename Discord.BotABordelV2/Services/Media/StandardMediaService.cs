using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models;
using Discord.BotABordelV2.Models.Results;

using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;

using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services.Media;

/// <summary>
/// A standard media service for handling media playback from multiple sources in a voice channel.
/// </summary>
/// <seealso cref="Discord.BotABordelV2.Services.Media.MediaServiceBase" />
/// <seealso cref="Discord.BotABordelV2.Interfaces.IMediaService" />
public class StandardMediaService(ILogger<StandardMediaService> logger,
                             IAudioService audioService,
                             IShadowBanService shadowBanService,
                             IOptionsMonitor<DiscordBotOptions> botOptions) : MediaServiceBase(logger, audioService, botOptions), IMediaService
{
    public override async Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel, PlaySource source)
    {
        if (string.IsNullOrEmpty(track))
            throw new ArgumentException($"'{nameof(track)}' cannot be null or empty.", nameof(track));

        var player = await GetPlayerAsync(channel);
        if (player is null)
            return new PlayTrackResult(PlayTrackStatus.PlayerUnavailable);

        var foundTrack = await LoadTrackFromSourceAsync(track, source);

        if (foundTrack is null)
        {
            logger.LogDebug("Track not found of name '{track}'", track);
            return new PlayTrackResult(PlayTrackStatus.NoTrackFound);
        }

        if (shadowBanService.IsTrackBanned(new(foundTrack.Identifier, foundTrack.Title, foundTrack.Uri)))
        {
            logger.LogDebug("Track shadow banned '{track}'", foundTrack.Title);
            return new PlayTrackResult(foundTrack ,PlayTrackStatus.TrackBanned);
        }

        var queuePos = await player.PlayAsync(foundTrack);
        logger.LogInformation("Playing track - {track}", foundTrack.Title);

        return new PlayTrackResult(foundTrack, queuePos);
    }
}