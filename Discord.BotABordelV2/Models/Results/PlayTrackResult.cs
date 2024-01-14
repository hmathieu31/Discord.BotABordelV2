using Lavalink4NET.Tracks;

using System.Diagnostics.CodeAnalysis;

namespace Discord.BotABordelV2.Models.Results;

public record PlayTrackResult
{
    public PlayTrackResult(LavalinkTrack track, int queuePosition = 0)
    {
        Track = track;
        QueuePosition = queuePosition;
        Status = queuePosition == 0
            ? PlayTrackStatus.Playing
            : PlayTrackStatus.Queued;
    }

    public PlayTrackResult(PlayTrackStatus status)
    {
        if (status is PlayTrackStatus.Playing
            || status is PlayTrackStatus.Queued)
        {
            throw new ArgumentException("Status must be a failure status", nameof(status));
        }

        Status = status;
    }

    public LavalinkTrack? Track { get; private set; }

    public int? QueuePosition { get; private set; }

    public PlayTrackStatus Status { get; private set; }

    [MemberNotNullWhen(true, nameof(Track))]
    [MemberNotNullWhen(true, nameof(QueuePosition))]
    public bool IsSuccess => Status is PlayTrackStatus.Playing or PlayTrackStatus.Queued;
}

public enum PlayTrackStatus
{
    Playing,
    Queued,
    NoTrackFound,
    UserNotInVoiceChannel,
    InternalException,
    PlayerUnavailable,
}