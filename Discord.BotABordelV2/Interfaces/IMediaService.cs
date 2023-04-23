using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Interfaces;

public interface IMediaService
{
    /// <summary>
    /// Plays the track.
    /// </summary>
    /// <param name="lava">The lava client.</param>
    /// <param name="track">The track name.</param>
    /// <param name="channel"></param>
    /// <returns>The response message</returns>
    Task<string> PlayTrackAsync(string track, DiscordChannel channel);
}