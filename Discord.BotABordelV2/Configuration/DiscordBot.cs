namespace Discord.BotABordelV2.Configuration;

public class DiscordBot
{
    public string LogLevel { get; set; } = null!;

    public bool LogUnknownEvents { get; set; }

    public List<EntrancesEvent> EntrancesEvents { get; set; } = null!;

    public string Token { get; set; } = null!;
}

public class EntrancesEvent
{
    public string Name { get; set; } = null!;

    public string TrackFilePath { get; set; } = null!;

    public ulong UserId { get; set; }
}