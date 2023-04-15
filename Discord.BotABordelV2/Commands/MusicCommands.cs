using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace Discord.BotABordelV2.Commands;

public class MusicCommands : ApplicationCommandModule
{
    [SlashCommand("play", "Play a song")]
    public async Task Play(InteractionContext ctx, [Option("song", "The song to play")][RemainingText] string song)
    {


        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();

        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn is null)
        {
            try
            {
                await JoinCurrentChannel(ctx);
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }
            catch (InvalidOperationException)
            {
                await ctx.CreateResponseAsync("You must be connected into a voice channel to invoke this command!");
                return;
            }
        }

        var loadResult = await node.Rest.GetTracksAsync(song);

        //If something went wrong on Lavalink's end                          
        if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

            //or it just couldn't find anything.
            || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        {
            await ctx.CreateResponseAsync($"Track search failed for {song}.");
            return;
        }

        var track = loadResult.Tracks.First();

        await conn.PlayAsync(track);

        string contentPattern =
        $@"Playing {track.Title}
        {track.Uri}";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                              new DiscordInteractionResponseBuilder()
                                .WithContent(contentPattern));
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

    private async Task JoinCurrentChannel(InteractionContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        var channel = ctx.Member.VoiceState?.Channel
            ?? throw new InvalidOperationException($"User {ctx.Member.DisplayName} was not connected into a voice channel when calling command");

        if (!lava.ConnectedNodes.Any())
        {
            await ctx.CreateResponseAsync("The Lavalink connection is not established");
            return;
        }

        var node = lava.ConnectedNodes.Values.First();

        if (channel.Type != ChannelType.Voice)
            throw new InvalidOperationException($"User {ctx.Member.DisplayName} was not connected into a voice channel when calling command");

        await node.ConnectAsync(channel);
    }
}