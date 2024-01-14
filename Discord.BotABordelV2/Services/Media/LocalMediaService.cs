using Discord.BotABordelV2.Models;

using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;

using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services.Media;

public class LocalMediaService(ILogger<LocalMediaService> logger,
                         IAudioService audioService) : MediaService(logger, audioService)
{
    /// <summary>
    /// Plays the track asynchronous.
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
    public override async Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel)
    {
        try
        {
            var player = await GetPlayerAsync(channel);

            if (player is null)
                return new PlayTrackResult(PlayTrackStatus.PlayerUnavailable);

            if (!File.Exists(track))
                throw new FileNotFoundException("Could not open find track at path", track);

            var loadedTrack = await AudioService.Tracks.LoadTrackAsync(
                Path.GetFullPath(track),
                new TrackLoadOptions(
                    StrictSearch: false
                    )
                ) ?? throw new InvalidOperationException($"Track not found");

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