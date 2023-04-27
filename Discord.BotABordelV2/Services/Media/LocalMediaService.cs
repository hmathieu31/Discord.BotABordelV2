using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.VoiceNext;
using MP3Sharp;
using static Discord.BotABordelV2.Exceptions.MediaExceptions;

namespace Discord.BotABordelV2.Services.Media;

public class LocalMediaService : MediaService
{

    public LocalMediaService(ILogger<LocalMediaService> logger, LavalinkExtension lava)
        : base(logger, lava)
    { }

    public override async Task<string> PlayTrackAsync(string track, DiscordChannel channel)
    {
        try
        {
            var conn = await JoinChannelAsync(channel)
                ?? throw new NullChannelConnectionException($"Could not connect to channel {channel}");

            conn.PlaybackFinished += Conn_PlaybackFinished;

            if (!File.Exists(track))
                throw new FileNotFoundException("Could not open find track at path", track);

            var loadResult = await conn.GetTracksAsync(new Uri("https://stbotarbordelfiles.blob.core.windows.net/blob-resourcesmedia/wide.mp3"));

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType != LavalinkLoadResultType.TrackLoaded)
            {
                throw new InvalidOperationException($"Track loading failed for track {track} - load result {loadResult.LoadResultType}");
            }

            var foundTrack = loadResult.Tracks.First()
                ?? throw new InvalidOperationException($"Tracks is empty");

            await conn.PlayAsync(foundTrack);
            _logger.LogInformation("Playing track {track}", foundTrack.Title);

            return $"Playing {foundTrack.Title}";

        }
        catch (InvalidChannelTypeException ex)
        {
            _logger.LogError(ex, "Error when trying to play localtrack at {track}", track);
            return "";
        }
    }

    private async Task Conn_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs args)
    {
        await DisconnectChannelAsync(sender);
    }

    private static async Task DisconnectChannelAsync(LavalinkGuildConnection connection)
    {
        await connection.DisconnectAsync();
    }
}