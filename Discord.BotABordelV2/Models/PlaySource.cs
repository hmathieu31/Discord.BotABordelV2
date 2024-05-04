using Discord.Interactions;

namespace Discord.BotABordelV2.Models;

public enum PlaySource
{
    YouTube,
    SoundCloud,
    // Spotify, TODO: Implement Spotify
    [Hide]
    Local
}