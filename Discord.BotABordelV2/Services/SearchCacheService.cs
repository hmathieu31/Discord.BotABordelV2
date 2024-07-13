namespace Discord.BotABordelV2.Services;

/// <summary>
/// A service that caches search results for a search query.
/// </summary>
public interface ISearchCacheService
{
    /// <summary>
    /// Adds the the tracks of a search query to the cache.
    /// </summary>
    /// <param name="queryResult">The list of URIs representing the search results.</param>
    /// <returns>The id of the added search results in the cache.</returns>
    Guid AddSearchResults(List<Uri> queryResult);

    /// <summary>
    /// Pops a searched URI from the cache. The entire corresponding search results are removed.
    /// </summary>
    /// <param name="queryKey">The index of the search results in the cache.</param>
    /// <param name="trackIndex">The index of the track in the search results.</param>
    /// <returns>The popped URI.</returns>
    Uri PopSearchedUri(Guid queryKey, int trackIndex);
}

public class SearchCacheService : ISearchCacheService
{
    private readonly Dictionary<Guid, List<Uri>> _searchResults = [];

    /// <inheritdoc/>
    public Guid AddSearchResults(List<Uri> queryResult)
    {
        Guid queryKey = Guid.NewGuid();
        _searchResults.Add(queryKey, queryResult);
        return queryKey;
    }

    /// <inheritdoc/>
    public Uri PopSearchedUri(Guid queryKey, int trackIndex)
    {
        var uri = _searchResults[queryKey][trackIndex];
        _searchResults.Remove(queryKey);
        return uri;
    }
}