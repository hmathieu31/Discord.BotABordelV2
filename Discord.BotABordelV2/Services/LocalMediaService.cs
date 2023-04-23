using Discord.BotABordelV2.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MP3Sharp;
using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services;

public class LocalMediaService : ILocalMediaService
{
    private readonly ILogger<LocalMediaService> _logger;
    private readonly DiscordClient _client;
    private VoiceNextConnection? _connection;

    public LocalMediaService(ILogger<LocalMediaService> logger, DiscordClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task PlayTrackAsync(string trackPath, DiscordChannel channel)
    {
        try
        {
            _connection = await JoinChannelAsync(channel)
                ?? throw new NullChannelConnectionException($"Could not connect to channel {channel}");

            if (!File.Exists(trackPath))
                throw new FileNotFoundException("Could not open find track at path", trackPath);

            using var stream = new MP3Stream(trackPath);
            var transmit = _connection.GetTransmitSink();

            await stream.CopyToAsync(transmit);

            await _connection.WaitForPlaybackFinishAsync();
            DisconnectChannel();
        }
        catch (InvalidChannelTypeException ex)
        {
            _logger.LogError(ex, "Error when trying to play localtrack at {trackPath}", trackPath);
        }
    }

    private async Task<VoiceNextConnection> JoinChannelAsync(DiscordChannel channel)
    {
        if (channel.Type is not ChannelType.Voice)
            throw new InvalidChannelTypeException(ChannelType.Voice);

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

    private void DisconnectChannel()
    {
        _connection?.Dispose();
    }
}