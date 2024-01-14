namespace Discord.BotABordelV2.Models;

public class PauseTrackResult
{
    public PauseTrackResult(PauseTrackStatus status)
    {
        Status = status;
    }

    public bool IsSuccess => Status is PauseTrackStatus.Paused;

    public PauseTrackStatus Status { get; private set; }
}

public enum PauseTrackStatus
{
    Paused,
    NothingPlaying,
    InternalException,
    UserNotInVoiceChannel,
    PlayerNotConnected
}