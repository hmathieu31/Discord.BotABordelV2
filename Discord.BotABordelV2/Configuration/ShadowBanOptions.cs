namespace Discord.BotABordelV2.Configuration;

public class ShadowBanOptions
{
    public const string SectionName = "ShadowBan";

    public List<string> BannedKeywords { get; set; } = [];

    public List<string> BannedUris { get; set; } = [];

    public List<SubTrack> SubstituteTracks { get; set; } = [];
}

public class SubTrack
{
    public string Uri { get; set; } = string.Empty;

    public double? StartSeconds { get; set; } = 0;

    public TimeSpan? StartTime => StartSeconds is not null ? TimeSpan.FromSeconds(StartSeconds.Value) : null;

    public double? EndSeconds { get; set; }

    public TimeSpan? EndTime => EndSeconds is not null ? TimeSpan.FromSeconds(EndSeconds.Value) : null;
}