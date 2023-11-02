namespace Discord.BotABordelV2.Models;
public class PauseTrackResult
{
    public PauseTrackResult(PauseTrackStatus status)
    {
        Status = status;
    }

    public PauseTrackStatus Status { get; private set; }

    public bool IsSuccess => Status is PauseTrackStatus.Paused;
}

public enum PauseTrackStatus
{
    Paused,
    NothingPlaying,
    InternalException,
    UserNotInVoiceChannel,
    PlayerNotConnected
}
