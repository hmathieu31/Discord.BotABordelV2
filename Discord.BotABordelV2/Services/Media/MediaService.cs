using Discord.BotABordelV2.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services.Media;
public class MediaService : IMediaService
{
    private readonly ILogger<MediaService> _logger;
    private readonly LavalinkExtension _lava;

    public MediaService(ILogger<MediaService> logger, LavalinkExtension lava)
    {
        _logger = logger;
        _lava = lava;
    }

    public async Task<string> PlayTrackAsync(string track, DiscordChannel channel)
    {
        if (string.IsNullOrEmpty(track))
            throw new ArgumentException($"'{nameof(track)}' cannot be null or empty.", nameof(track));

        var node = _lava.ConnectedNodes.Values.First()
            ?? throw new InvalidOperationException("Not connected to a lava node");

        var conn = node.GetGuildConnection(channel.Guild);
        if (conn is null)
        {
            try
            {
                await JoinChannel(channel, node);
                conn = node.GetGuildConnection(channel.Guild);
            }
            catch (InvalidChannelTypeException ex)
            {
                return $"Channel must be {ex.RequiredChannelType}";
            }
        }

        var loadResults = await node.Rest.GetTracksAsync(track);

        //If something went wrong on Lavalink's end                          
        if (loadResults.LoadResultType == LavalinkLoadResultType.LoadFailed
            //or it just couldn't find anything.
            || loadResults.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            return $"Track search failed for {track}.";
        }

        var foundTrack = loadResults.Tracks.First();

        await conn.PlayAsync(foundTrack);

        return
 $@"Playing {foundTrack.Title}
{foundTrack.Uri}";
    }

    private async Task JoinChannel(DiscordChannel channel, LavalinkNodeConnection node)
    {
        if (channel.Type is not DSharpPlus.ChannelType.Voice)
            throw new InvalidChannelTypeException(DSharpPlus.ChannelType.Voice);

        try
        {

            await channel.ConnectAsync(node);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while trying to join channel");
            throw;
        }
    }
}
