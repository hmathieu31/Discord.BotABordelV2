using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.BotABordelV2.Commands;

public class SlashCommandDemo : ApplicationCommandModule
{
    [SlashCommand("test", "Slash test command")]
    public async Task TestCommand(InteractionContext context)
    {
        await context.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent("Wesh Wesh Canapèche")
            );
    }
}
