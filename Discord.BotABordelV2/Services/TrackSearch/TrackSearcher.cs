using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Services.TrackSearch;

public class TrackSearcher
{
    public async Task<LavalinkTrack?> SearchTrackAsync(LavalinkGuildConnection connection, string trackQuery)
    {
        var loadResults = await connection.GetTracksAsync(trackQuery);
        //If something went wrong on Lavalink's end
        if (loadResults.LoadResultType == LavalinkLoadResultType.LoadFailed
            //or it just couldn't find anything.
            || loadResults.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            return null;
        }

        return loadResults.Tracks.FirstOrDefault();
    }
}