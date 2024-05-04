using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Models;
using Discord.BotABordelV2.Models.Results;

using Lavalink4NET;

using Microsoft.Extensions.Options;

using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services.Media;

/// <summary>
/// A media service for handling grand entrances events media playbacks.
/// </summary>
/// <seealso cref="Discord.BotABordelV2.Services.Media.MediaServiceBase" />
public class GrandEntranceMediaService(ILogger<GrandEntranceMediaService> logger,
                         IAudioService audioService,
                         IOptionsMonitor<DiscordBotOptions> botOptions) : MediaServiceBase(logger, audioService, botOptions)
{
    /// <summary>
    /// Plays the track immediately pausing tracks if any and resuming after.
    /// </summary>
    /// <param name="track">The track path.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>A message containing the played track</returns>
    /// <exception cref="Discord.BotABordelV2.Exceptions.MediaExceptions.NullChannelConnectionException">
    /// Could not connect to channel {channel}
    /// </exception>
    /// <exception cref="System.IO.FileNotFoundException">Could not open find track at path</exception>
    /// <exception cref="System.InvalidOperationException">
    /// Track loading failed for track {track} - load result {loadResult.LoadResultType} or Tracks
    /// is empty
    /// </exception>
    public override async Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel, PlaySource source)
    {
        try
        {
            if (string.IsNullOrEmpty(track))
                throw new ArgumentException($"'{nameof(track)}' cannot be null or empty.", nameof(track));

            var player = await GetPlayerAsync(channel);
            if (player is null)
                return new PlayTrackResult(PlayTrackStatus.PlayerUnavailable);

            var loadedTrack = await LoadTrackFromSourceAsync(track, source);
            if (loadedTrack is null)
            {
                logger.LogDebug("Track not found of name '{track}'", track);
                return new PlayTrackResult(PlayTrackStatus.NoTrackFound);
            }

            await player.PlayEventTrackAsync(loadedTrack);
            logger.LogInformation("Playing track {track}", loadedTrack.Title);

            return new PlayTrackResult(loadedTrack);
        }
        catch (InvalidChannelTypeException ex)
        {
            logger.LogError(ex, "Error when trying to play local track at {track}", track);
            return new PlayTrackResult(PlayTrackStatus.InternalException);
        }
    }
}