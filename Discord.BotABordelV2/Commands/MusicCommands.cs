using Discord.BotABordelV2.Constants;
using Discord.BotABordelV2.Models;
using Discord.BotABordelV2.Services.Media;
using Discord.Interactions;


namespace Discord.BotABordelV2.Commands;

[RequireContext(ContextType.Guild)]
public sealed class MusicCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly StreamingMediaService _mediaService;

    public MusicCommands(StreamingMediaService mediaService)
    {
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
    }

    [SlashCommand("play", "Play a song", runMode: RunMode.Async)]
    public async Task Play(
        [Summary("Song", "The song to play")] string song
        )
    {
        await DeferAsync();

        IVoiceChannel? channel = (Context.User as IGuildUser)?.VoiceChannel;
        if (channel is null)
        {
            await FollowupAsync(MessageResponses.UserNotConnected);
            return;
        }

        var result = await _mediaService.PlayTrackAsync(song, channel);
        if (result.IsSuccess)
        {
            var response = result.Status switch
            {
                Models.PlayTrackStatus.Playing => $"🔈  Playing {result.Track.Title} ({result.Track.Uri})",
                Models.PlayTrackStatus.Queued => $"🔈  Added to queue {result.Track.Title} ({result.Track.Uri})",
                _ => throw new NotImplementedException(),
            };

            await FollowupAsync(response);
        }
        else
        {
            var error = result.Status switch
            {
                PlayTrackStatus.NoTrackFound => $"😖  No results.",
                PlayTrackStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
                _ => MessageResponses.InternalEx
            };

            await FollowupAsync(error);
        }
    }

    [SlashCommand("stop", "Stop the player and clears the queue", runMode: RunMode.Async)]
    public async Task Stop()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await _mediaService.StopPlayerAsync(channel)).Status switch
        {
            StopPlayerStatus.Stopped => "🛑  Stopped player and cleared queue",
            StopPlayerStatus.InternalException => MessageResponses.InternalEx,
            StopPlayerStatus.NothingPlaying => MessageResponses.NothingPlaying,
            StopPlayerStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
            StopPlayerStatus.PlayerNotConnected => MessageResponses.NothingPlaying,
            _ => MessageResponses.InternalEx,
        };
        await RespondAsync(response);
    }

    [SlashCommand("pause", "Pause current track", runMode: RunMode.Async)]
    public async Task Pause()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await _mediaService.PauseTrackAsync(channel)).Status switch
        {
            PauseTrackStatus.Paused => "⏸️  Paused.",
            PauseTrackStatus.NothingPlaying => MessageResponses.NothingPlaying,
            PauseTrackStatus.InternalException => MessageResponses.InternalEx,
            PauseTrackStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
            PauseTrackStatus.PlayerNotConnected => MessageResponses.NothingPlaying,
            _ => MessageResponses.InternalEx,
        };

        await RespondAsync(response);
    }

    [SlashCommand("resume", "Resume current paused track", runMode: RunMode.Async)]
    public async Task Resume()
    {
        var channel = (Context.User as IGuildUser)?.VoiceChannel;

        if (channel is null)
        {
            await RespondAsync(MessageResponses.UserNotConnected);
            return;
        }

        var response = (await _mediaService.ResumeTrackAsync(channel)).Status switch
        {
            ResumeTrackStatus.Resumed => "⏯️  Resumed",
            ResumeTrackStatus.PlayerNotPaused => MessageResponses.NothingPaused,
            ResumeTrackStatus.InternalException => MessageResponses.InternalEx,
            ResumeTrackStatus.UserNotInVoiceChannel => MessageResponses.UserNotConnected,
            ResumeTrackStatus.PlayerNotConnected => MessageResponses.NothingPaused,
            _ => MessageResponses.InternalEx,
        };
        await RespondAsync(response);
    }
}
