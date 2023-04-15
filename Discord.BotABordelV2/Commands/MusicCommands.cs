using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.BotABordelV2.Commands;
public class MusicCommands : ApplicationCommandModule
{
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

    [SlashCommand("play", "Play a song")]
    public async Task Play(InteractionContext ctx, [Option("song", "The song to play")][RemainingText] string song)
    {
        try
        {
            await JoinCurrentChannel(ctx);
        }
        catch (InvalidOperationException)
        {
            await ctx.CreateResponseAsync("You must be connected into a voice channel to invoke this command!");
            return;
        }

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected");
            return;
        }

        var loadResult = await node.Rest.GetTracksAsync(song);
        var track = loadResult.Tracks.First();
        await conn.PlayAsync(track);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                              new DiscordInteractionResponseBuilder()
                                .WithContent($"Playing {track.Uri}"));
    }
}
