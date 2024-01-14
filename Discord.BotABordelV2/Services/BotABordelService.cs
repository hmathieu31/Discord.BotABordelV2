using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Options;

using System.Reflection;

namespace Discord.BotABordelV2.Services;

public class BotABordelService(ILogger<BotABordelService> logger,
                         IGrandEntranceService grandEntranceService,
                         DiscordSocketClient discordSocketClient,
                         InteractionService interactionService,
                         IOptions<DiscordBot> options,
                         IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        discordSocketClient.InteractionCreated += InteractionCreated;
        discordSocketClient.Ready += ClientReady;
        discordSocketClient.Log += LogAsync;

        discordSocketClient.UserVoiceStateUpdated += UserVoiceStateUpdated;

        var token = options.Value.Token;
        await discordSocketClient.LoginAsync(TokenType.Bot, token)
                                  .ConfigureAwait(false);

        await discordSocketClient.StartAsync()
                                  .ConfigureAwait(false);

        logger.LogInformation("Discord client stared");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        discordSocketClient.InteractionCreated -= InteractionCreated;
        discordSocketClient.Ready -= ClientReady;

        discordSocketClient.UserVoiceStateUpdated -= UserVoiceStateUpdated;

        await discordSocketClient
            .StopAsync()
            .ConfigureAwait(false);

        logger.LogInformation("Discord Client disconnected");
    }

    private async Task ClientReady()
    {
        await interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), services)
                                 .ConfigureAwait(false);

        await interactionService.RegisterCommandsGloballyAsync()
                                 .ConfigureAwait(false);
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var interactionCtx = new SocketInteractionContext(discordSocketClient, interaction);
            await interactionService.ExecuteCommandAsync(interactionCtx, services);
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction
            // acknowledgement will persist. It is a good idea to delete the original response, or
            // at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private Task LogAsync(LogMessage log)
    {
        logger.LogDebug("{msg}", log.ToString());
        return Task.CompletedTask;
    }

    private Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
    {
        try
        {
            _ = grandEntranceService.TriggerCustomEntranceScenarioAsync(user, newVoiceState);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An Exception occurred in the OnUserConnection Event");
            return Task.CompletedTask;
        }
    }
}