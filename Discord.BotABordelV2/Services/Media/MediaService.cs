using Discord.BotABordelV2.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services.Media;

public abstract class MediaService : IMediaService
{
    protected readonly ILogger _logger;
    private readonly LavalinkExtension _lava;

    protected MediaService(ILogger logger, LavalinkExtension lava)
    {
        _logger = logger;
        _lava = lava;
    }

    public abstract Task<string> PlayTrackAsync(string track, DiscordChannel channel);

    protected async Task<LavalinkGuildConnection> JoinChannelAsync(DiscordChannel channel)
    {
        if (channel.Type is not DSharpPlus.ChannelType.Voice)
            throw new InvalidChannelTypeException(DSharpPlus.ChannelType.Voice);

        if (_lava.GetGuildConnection(channel.Guild) is not null)
            return _lava.GetGuildConnection(channel.Guild);

        try
        {
            var node = _lava.GetIdealNodeConnection();
            return await node.ConnectAsync(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while trying to join channel");
            throw;
        }
    }
}