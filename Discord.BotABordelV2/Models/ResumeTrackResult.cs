namespace Discord.BotABordelV2.Models;
public class ResumeTrackResult
{
    public ResumeTrackStatus Status { get; }

    public ResumeTrackResult(ResumeTrackStatus status)
    {
        Status = status;
    }

    public bool IsSuccess => Status is ResumeTrackStatus.Resumed;

}

public enum ResumeTrackStatus
{
    Resumed,
    PlayerNotPaused,
    InternalException,
    UserNotInVoiceChannel,
    PlayerNotConnected
}
