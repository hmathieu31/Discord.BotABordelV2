using Discord.BotABordelV2.Interfaces;
using DSharpPlus;
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
    private DiscordClient? _client;

    public LocalMediaService(ILogger<LocalMediaService> logger)
    {
        _logger = logger;
    }

    private async Task<VoiceNextConnection> JoinChannelAsync(DiscordChannel channel)
    {
        if (channel.Type is not DSharpPlus.ChannelType.Voice)
            throw new InvalidChannelTypeException(DSharpPlus.ChannelType.Voice);

        try
        {
            return await channel.ConnectAsync();
        }
        catch (InvalidOperationException ex) when (ex.Message == "VoiceNext is already connected in this guild.")
        {
            _logger.LogDebug("A VoiceNext connection is already established");
            return _client.GetVoiceNext().GetConnection(channel.Guild);
        }
    }

    private void DisconnectChannel(VoiceNextConnection connection)
    {
        connection.Disconnect();
    }

    public async Task PlayTrackAsync(DiscordClient client, string trackPath, DiscordChannel channel)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));

        _client = client;
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
