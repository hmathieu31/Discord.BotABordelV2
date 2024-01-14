namespace Discord.BotABordelV2.Models;

public class StopPlayerResult
{
    public StopPlayerResult(StopPlayerStatus status)
    {
        Status = status;
    }

    public bool IsSuccess => Status is StopPlayerStatus.Stopped;

    public StopPlayerStatus Status { get; private set; }
}

public enum StopPlayerStatus
{
    Stopped,
    InternalException,
    NothingPlaying,
    UserNotInVoiceChannel,
    PlayerNotConnected
}