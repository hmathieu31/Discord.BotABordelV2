namespace Discord.BotABordelV2.Configuration;

public class EmotesOptions
{
    public const string SectionName = "EmotesIds";

    public required string YouTubeEmoteId { get; set; }

    public required string SoundCloudEmoteId { get; set; }
}