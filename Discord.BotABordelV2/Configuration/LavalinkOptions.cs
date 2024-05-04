﻿namespace Discord.BotABordelV2.Configuration;

public class LavalinkOptions
{
    public const string SectionName = "Lavalink";

    public string Host { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Port { get; set; }
}