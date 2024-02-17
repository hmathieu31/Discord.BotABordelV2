namespace Discord.BotABordelV2.Configuration;

public class DiscordBotOptions
{
    public const string ConfigSectionName = "DiscordBot";

    public List<EntrancesEvent> EntrancesEvents { get; set; } = null!;

    public ulong GuildId { get; set; }

    public string LogLevel { get; set; } = null!;

    public bool LogUnknownEvents { get; set; }

    public string Token { get; set; } = null!;

    public int TracksReturnedPerSearch { get; set; }
}

public class EntrancesEvent
{
    public string Name { get; set; } = null!;

    public string TrackFilePath { get; set; } = null!;

    public ulong UserId { get; set; }
}