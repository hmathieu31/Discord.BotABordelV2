using Discord.BotABordelV2.Interfaces;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace Discord.BotABordelV2.Commands;

public class MusicCommands : ApplicationCommandModule
{
    private readonly IMediaService _mediaService;
    private readonly ILocalMediaService _localMediaService;
    private readonly IConfiguration _configuration;

    public MusicCommands(IMediaService mediaService,
                         ILocalMediaService localMediaService,
                         IConfiguration configuration)
    {
        _mediaService = mediaService;
        _localMediaService = localMediaService;
        _configuration = configuration;
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play(InteractionContext ctx, [Option("song", "The song to play")][RemainingText] string song)
    {
        var lava = ctx.Client.GetLavalink();

        var channel = ctx.Member.VoiceState?.Channel;
        string response;
        if (channel is null)
        {
            response = "You must be in a voice channel";
        }
        else
        {
            response = await _mediaService.PlayTrackAsync(lava, song, channel);
        }

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                              new DiscordInteractionResponseBuilder()
                                .WithContent(response));
    }

    [SlashCommand("stop", "Stop the currently played track")]
    public async Task Stop(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.FirstOrDefault();
        var conn = node?.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected");
            return;
        }

        var currentTrack = conn.CurrentState.CurrentTrack;
        if (currentTrack is null)
        {
            await ctx.CreateResponseAsync("Nothing is playing");
            return;
        }

        await conn.StopAsync();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                      new DiscordInteractionResponseBuilder()
                        .WithContent($"Stopped {currentTrack.Title}"));
    }

    [SlashCommand("pause", "Pause the currently played track")]
    public async Task Pause(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.FirstOrDefault();
        var conn = node?.GetGuildConnection(ctx.Member.VoiceState.Guild);
        if (conn is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected");
            return;
        }

        var currentTrack = conn.CurrentState.CurrentTrack;
        if (currentTrack is null)
        {
            await ctx.CreateResponseAsync("Nothing is playing");
            return;
        }

        await conn.PauseAsync();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                 new DiscordInteractionResponseBuilder()
                                                        .WithContent($"Paused {currentTrack.Title}"));
    }

    [SlashCommand("resume", "Resume the currently paused track")]
    public async Task Resume(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.FirstOrDefault();
        var conn = node?.GetGuildConnection(ctx.Member.VoiceState.Guild);
        if (conn is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected");
            return;
        }

        var currentTrack = conn.CurrentState.CurrentTrack;
        if (currentTrack is null)
        {
            await ctx.CreateResponseAsync("No paused track to resume");
            return;
        }

        await conn.ResumeAsync();
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                            new DiscordInteractionResponseBuilder()
                                                .WithContent($"Resumed {conn.CurrentState.CurrentTrack.Title}"));
    }

    [SlashCommand("debug", "Debug")]
    public async Task Debug(InteractionContext ctx)
    {
        var trackPath = _configuration["WideRatio:TrackFilePath"];
        if (trackPath is null)
            return;

        var channel = ctx.Member.VoiceState?.Channel;
        if (channel is null)
            return;

        await _localMediaService.PlayTrackAsync(trackPath, channel);
    }
}