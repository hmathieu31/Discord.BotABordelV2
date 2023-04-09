using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Discord.BotABordelV2.Commands;

public class GreetingCommands : BaseCommandModule
{
    [Command("hello")]
    public async Task GreetCommandAsync(CommandContext context)
    {
        await context.TriggerTypingAsync();
        await context.RespondAsync($"Hello {context.User.Mention} Wesh Wesh Canapèche ou quoi!");
    }
}