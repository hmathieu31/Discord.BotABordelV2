using DSharpPlus.Entities;

namespace Discord.BotABordelV2.Interfaces;

public interface ILocalMediaService
{
    Task PlayTrackAsync(string trackPath, DiscordChannel channel);
}