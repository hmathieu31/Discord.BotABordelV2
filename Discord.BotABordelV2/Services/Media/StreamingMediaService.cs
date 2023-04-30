using Discord.BotABordelV2.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Services.Media;

public class StreamingMediaService : MediaService, IMediaService
{
    public StreamingMediaService(ILogger<StreamingMediaService> logger, LavalinkExtension lava)
        : base(logger, lava)
    { }

    public override async Task<string> PlayTrackAsync(string track, DiscordChannel channel)
    {
        if (string.IsNullOrEmpty(track))
            throw new ArgumentException($"'{nameof(track)}' cannot be null or empty.", nameof(track));

        var conn = await JoinChannelAsync(channel);
        var loadResults = await conn.GetTracksAsync(track);

        //If something went wrong on Lavalink's end
        if (loadResults.LoadResultType == LavalinkLoadResultType.LoadFailed
            //or it just couldn't find anything.
            || loadResults.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            return $"Track search failed for {track}.";
        }

        var foundTrack = loadResults.Tracks.First();

        await conn.PlayAsync(foundTrack);

        return
 $@"Playing {foundTrack.Title}
{foundTrack.Uri}";
    }
}