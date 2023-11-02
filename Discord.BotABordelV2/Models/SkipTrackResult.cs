
using Lavalink4NET.Tracks;

namespace Discord.BotABordelV2.Models;
public class SkipTrackResult
{
    public SkipTrackResult(LavalinkTrack nextTrack)
    {
        NextTrack = nextTrack;
        Status = SkipTrackStatus.Skipped;
    }

    public SkipTrackResult(SkipTrackStatus status)
    {
        Status = status;
    }

    public LavalinkTrack? NextTrack { get; private set; }

    public SkipTrackStatus Status { get; private set; }

    public bool IsSuccess => Status is SkipTrackStatus.Skipped or SkipTrackStatus.FinishedQueue;
}

public enum SkipTrackStatus
{
    Skipped,
    FinishedQueue,
    InternalException,
    NothingPlaying,
    UserNotInVoiceChannel,
    PlayerNotConnected
}