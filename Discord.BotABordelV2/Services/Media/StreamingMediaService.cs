using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services.TrackSearch;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Services.Media;

public class StreamingMediaService : MediaService, IMediaService
{
    private readonly TrackSearcherStrategy _trackSearcher;

    public StreamingMediaService(ILogger<StreamingMediaService> logger,
                                 LavalinkExtension lava,
                                 TrackSearcherStrategy trackSearch)
        : base(logger, lava)
    {
        _trackSearcher = trackSearch;
    }

    public override async Task<string> PlayTrackAsync(string track, DiscordChannel channel)
    {
        if (string.IsNullOrEmpty(track))
            throw new ArgumentException($"'{nameof(track)}' cannot be null or empty.", nameof(track));

        var conn = await JoinChannelAsync(channel);

        var foundTrack = await _trackSearcher.SearchTrackAsync(conn, track);
        if (foundTrack is null)
        {
            _logger.LogDebug("Track not found of name", track);
            return $"Track search failed for {track}.";
        }

        await conn.PlayAsync(foundTrack);
        _logger.LogInformation("Playing track - {track}", foundTrack.Title);

        return
 $@"Playing {foundTrack.Title}
{foundTrack.Uri}";
    }
}