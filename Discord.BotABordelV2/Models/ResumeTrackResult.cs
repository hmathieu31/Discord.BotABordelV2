namespace Discord.BotABordelV2.Models;

public class ResumeTrackResult
{
    public ResumeTrackResult(ResumeTrackStatus status)
    {
        Status = status;
    }

    public bool IsSuccess => Status is ResumeTrackStatus.Resumed;

    public ResumeTrackStatus Status { get; }
}

public enum ResumeTrackStatus
{
    Resumed,
    PlayerNotPaused,
    InternalException,
    UserNotInVoiceChannel,
    PlayerNotConnected
}