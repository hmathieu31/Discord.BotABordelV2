using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Services.TrackSearch;

/// <summary>
/// This class searches for a track based on a query provided by the client.
/// </summary>
public class TrackSearcherStrategy
{
    /// <summary>
    /// Searches for a track based on provided query.
    /// </summary>
    /// <param name="connection">The Lavalink guild connection.</param>
    /// <param name="trackQuery">The track query to search for. It can be a Uri or a string.</param>
    /// <returns>
    /// Returns the first found <see cref="LavalinkTrack"/> or <c>null</c> if nothing was found or
    /// an error occurred.
    /// </returns>
    public async Task<LavalinkTrack?> SearchTrackAsync(LavalinkGuildConnection connection, string trackQuery)
    {
        if (Uri.IsWellFormedUriString(trackQuery, UriKind.Absolute))
        {
            var trackUri = new Uri(trackQuery);
            return await SearchTrackInternalAsync(connection, trackUri);
        }
        else
        {
            return await SearchTrackInternalAsync(connection, trackQuery);
        }
    }

    private async Task<LavalinkTrack?> SearchTrackInternalAsync(LavalinkGuildConnection connection, string trackName)
    {
        var loadResults = await connection.GetTracksAsync(trackName);
        //If something went wrong on Lavalink's end
        if (loadResults.LoadResultType == LavalinkLoadResultType.LoadFailed
            //or it just couldn't find anything.
            || loadResults.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            return null;
        }

        return loadResults.Tracks.FirstOrDefault();
    }

    private async Task<LavalinkTrack?> SearchTrackInternalAsync(LavalinkGuildConnection connection, Uri trackUri)
    {
        var loadResults = await connection.GetTracksAsync(trackUri);
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
