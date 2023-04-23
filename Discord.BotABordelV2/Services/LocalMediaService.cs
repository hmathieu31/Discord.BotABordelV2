using Discord.BotABordelV2.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MP3Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services;
public class LocalMediaService : ILocalMediaService
{
    private readonly ILogger<LocalMediaService> _logger;

    public LocalMediaService(ILogger<LocalMediaService> logger)
    {
        _logger = logger;
    }

    private async Task<VoiceNextConnection> JoinChannelAsync(DiscordChannel channel)
    {
        if (channel.Type is not DSharpPlus.ChannelType.Voice)
            throw new InvalidChannelTypeException(DSharpPlus.ChannelType.Voice);

        return await channel.ConnectAsync();
    }

    private void DisconnectChannel(VoiceNextConnection connection)
    {
        connection.Disconnect();
    }

    public async Task PlayTrackAsync(string trackPath, DiscordChannel channel)
    {
        try
        {
            var connection = await JoinChannelAsync(channel)
                ?? throw new NullChannelConnectionException($"Could not connect to channel {channel}");
            if (!File.Exists(trackPath))
                throw new FileNotFoundException("Could not open find track at path", trackPath);

            using var stream = new MP3Stream(trackPath);
            var transmit = connection.GetTransmitSink();

            await stream.CopyToAsync(transmit);
        }
        catch (InvalidChannelTypeException ex)
        {
            _logger.LogError(ex, "Error when trying to play localtrack at {trackPath}", trackPath);
        }
    }
}
