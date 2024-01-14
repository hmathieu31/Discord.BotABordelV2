using Discord.BotABordelV2.Models.Tracks;

namespace Discord.BotABordelV2.Models.Results;

public class GetQueueResult
{
    public GetQueueResult(DisplayQueueStatus status)
    {
        Status = status;
    }

    public GetQueueResult(List<TrackQueueItem> queuedTracks)
    {
        Status = DisplayQueueStatus.Displaying;
        QueuedTracks = queuedTracks;
    }

    public List<TrackQueueItem>? QueuedTracks { get; init; }

    public bool IsSuccess => Status is DisplayQueueStatus.Displaying;

    public DisplayQueueStatus Status { get; private set; }
}

public enum DisplayQueueStatus
{
    Displaying,
    InternalException,
    NothingPlaying,
    UserNotInVoiceChannel,
    PlayerNotConnected
}