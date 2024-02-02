using System.Diagnostics.CodeAnalysis;
using Lavalink4NET.Tracks;

namespace Discord.BotABordelV2;

public record SearchTrackResult
{
    public SearchTrackResult(IEnumerable<LavalinkTrack> foundTracks)
    {
        if (!foundTracks.Any())
        {
            Status = SearchTrackStatus.NoTrackFound;
            return;
        }

        FoundTracks = foundTracks;
    }

    public SearchTrackResult(SearchTrackStatus status)
    {
        if (status is SearchTrackStatus.TracksFound)
        {
            throw new ArgumentException("Status must be a failure status and is not:", nameof(status));
        }

        Status = status;
    }

    public IEnumerable<LavalinkTrack> FoundTracks { get; private set; } = [];

    public SearchTrackStatus Status { get; private set; }

    public bool IsSuccess => Status is SearchTrackStatus.TracksFound;
}

public enum SearchTrackStatus
{
    TracksFound,
    NoTrackFound,
    InternalException,
}