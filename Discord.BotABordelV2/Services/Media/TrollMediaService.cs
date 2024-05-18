using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Models;
using Discord.BotABordelV2.Models.Results;

using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;

using Microsoft.Extensions.Options;

using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services.Media;

public class TrollMediaService(IAudioService audioService,
            IOptionsMonitor<DiscordBotOptions> options,
            IOptionsMonitor<ShadowBanOptions> banOptions,
            ILogger<TrollMediaService> logger) : MediaServiceBase(logger, audioService, options)
{
    /// <summary>
    /// Plays a troll track in the specified channel instead of specified track.
    /// </summary>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public override async Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel, PlaySource source)
    {
        try
        {
            var player = await GetPlayerAsync(channel, false)
                ?? throw new NullChannelConnectionException();

            var subTracks = banOptions.CurrentValue.SubstituteTracks;

            var subTrackOpt = subTracks.Count > 0
                ? subTracks[new Random().Next(subTracks.Count)]
                : null;

            if (subTrackOpt is null)
            {
                logger.LogWarning("No substitute tracks found");
                return new PlayTrackResult(PlayTrackStatus.NoTrackFound);
            }

            var subTrack = await AudioService.Tracks.LoadTrackAsync(subTrackOpt.Uri, searchMode: TrackSearchMode.YouTube);
            if (subTrack is null)
            {
                logger.LogWarning("Substitute track not found of uri '{uri}'", subTrackOpt);
                return new PlayTrackResult(PlayTrackStatus.NoTrackFound);
            }

            var queuePos = await player.PlayAsync(subTrack, true, new TrackPlayProperties(subTrackOpt.StartTime, subTrackOpt.EndTime));
            logger.LogInformation("Playing / Adding subst track - {track}", subTrack.Title);

            return new PlayTrackResult(subTrack, queuePos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception when trying to Troll Play");
            return new PlayTrackResult(PlayTrackStatus.InternalException);
        }
    }
}