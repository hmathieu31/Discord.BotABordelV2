using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Services;

public interface IMediaService
{
    /// <summary>
    /// Plays the track.
    /// </summary>
    /// <param name="lava">The lava client.</param>
    /// <param name="track">The track name.</param>
    /// <param name="channel"></param>
    /// <returns>The response message</returns>
    Task<string> PlayTrackAsync(LavalinkExtension lava, string track, DiscordChannel channel);

    /// <summary>
    /// Determines whether the specified lava client is connected to a voice chanel.
    /// </summary>
    /// <param name="lava">The lava.</param>
    /// <param name="discordGuild">The discord guild. Must be <see cref="DSharpPlus.ChannelType.Voice"/> type</param>
    /// <returns><c>true</c> if connected to a voice channel, <c>false</c> otherwise</returns>
    bool IsConnectedToGuild(LavalinkExtension lava, DiscordGuild discordGuild);
}