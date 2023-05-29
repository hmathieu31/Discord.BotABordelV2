using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services.Media;

using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace Discord.BotABordelV2.Commands;

public class MusicCommands : ApplicationCommandModule
{
    public MusicCommands()
    {
    }

    [SlashCommand("play", "Play a song")]
    public async Task Play(InteractionContext ctx, [Option("song", "The song to play")][RemainingText] string song)
    {
        using var scope = ctx.Services.CreateScope();

        var channel = ctx.Member.VoiceState?.Channel;
        string response;
        if (channel is null)
        {
            response = "You must be in a voice channel";
        }
        else
        {
            response = await GetScopedStreamingService(scope).PlayTrackAsync(song, channel);
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

    private IMediaService GetScopedStreamingService(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<StreamingMediaService>();
}