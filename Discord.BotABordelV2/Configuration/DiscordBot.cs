namespace Discord.BotABordelV2.Configuration;

public class DiscordBot
{
    public string LogLevel { get; set; } = null!;

    public bool LogUnknownEvents { get; set; }

    public EntrancesEvents EntrancesEvents { get; set; } = null!;

    public string Token { get; set; } = null!;
}

public class EntrancesEvents
{
    public WideRatio WideRatio { get; set; } = null!;
}

public class WideRatio
{
    public string TrackUrl { get; set; } = null!;

    public string TrackFilePath { get; set; } = null!;

    public ulong RatioId { get; set; }
}