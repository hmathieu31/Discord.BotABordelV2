using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Options;

using System.Reflection;

namespace Discord.BotABordelV2.Services;

public class BotABordelService : IHostedService
{
    private readonly ILogger<BotABordelService> _logger;
    private readonly IGrandEntranceService _grandEntranceService;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly InteractionService _interactionService;
    private readonly IOptions<DiscordBot> _options;
    private readonly IServiceProvider _services;

    public BotABordelService(ILogger<BotABordelService> logger,
                             IGrandEntranceService grandEntranceService,
                             DiscordSocketClient discordSocketClient,
                             InteractionService interactionService,
                             IOptions<DiscordBot> options,
                             IServiceProvider services)
    {
        _logger = logger;
        _grandEntranceService = grandEntranceService;
        _discordSocketClient = discordSocketClient;
        _interactionService = interactionService;
        _options = options;
        _services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discordSocketClient.InteractionCreated += InteractionCreated;
        _discordSocketClient.Ready += ClientReady;
        _discordSocketClient.Log += LogAsync;

        _discordSocketClient.UserVoiceStateUpdated += UserVoiceStateUpdated;

        var token = _options.Value.Token;
        await _discordSocketClient.LoginAsync(TokenType.Bot, token)
                                  .ConfigureAwait(false);

        await _discordSocketClient.StartAsync()
                                  .ConfigureAwait(false);

        _logger.LogInformation("Discord client stared");
    }

    private Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
    {
        try
        {
            _ = _grandEntranceService.TriggerCustomEntranceScenarioAsync(user, newVoiceState);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An Exception occured in the OnUserConnection Event");
            return Task.CompletedTask;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _discordSocketClient.InteractionCreated -= InteractionCreated;
        _discordSocketClient.Ready -= ClientReady;

        _discordSocketClient.UserVoiceStateUpdated -= UserVoiceStateUpdated;

        await _discordSocketClient
            .StopAsync()
            .ConfigureAwait(false);

        _logger.LogInformation("Discord Client disconnected");
    }

    private async Task ClientReady()
    {
        await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services)
                                 .ConfigureAwait(false);

        await _interactionService.RegisterCommandsGloballyAsync()
                                 .ConfigureAwait(false);

    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var interactionCtx = new SocketInteractionContext(_discordSocketClient, interaction);
            await _interactionService.ExecuteCommandAsync(interactionCtx, _services);
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogDebug("{msg}",log.ToString());
        return Task.CompletedTask;
    }
}