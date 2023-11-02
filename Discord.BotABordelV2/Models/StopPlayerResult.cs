namespace Discord.BotABordelV2.Models;

public class StopPlayerResult
{
    public StopPlayerResult(StopPlayerStatus status)
    {
        Status = status;
    }

    public StopPlayerStatus Status { get; private set; }

    public bool IsSuccess => Status is StopPlayerStatus.Stopped;
}

public enum StopPlayerStatus
{
    Stopped,
    InternalException,
    NothingPlaying,
    UserNotInVoiceChannel,
    PlayerNotConnected
}