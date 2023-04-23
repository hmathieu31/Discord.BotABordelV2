using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.BotABordelV2.Configuration;
public class AppSettings
{
    public required WideRatio WideRatio { get; set; }

    public required DiscordBot DiscordBot { get; set; }
}

public class WideRatio
{
    public required string TrackUrl { get; set; }
}

public class DiscordBot
{
    public required string Token { get; set; }

    public LogLevel LogLevel { get; set; }

    public bool LogUnknownEvents { get; set; }
}