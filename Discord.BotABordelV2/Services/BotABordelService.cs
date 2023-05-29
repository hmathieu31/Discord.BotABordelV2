using Discord.BotABordelV2.Interfaces;

using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Services;

public class BotABordelService : IHostedService
{
    private readonly ILogger<BotABordelService> _logger;
    private readonly DiscordClient _discordClient;
    private readonly LavalinkExtension _lavalink;
    private readonly LavalinkConfiguration _lavalinkConfiguration;
    private readonly IServiceScopeFactory _scopeFactory;

    public BotABordelService(ILogger<BotABordelService> logger,
                             DiscordClient discordClient,
                             LavalinkExtension lavalink,
                             LavalinkConfiguration lavalinkConfiguration,
                             IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _discordClient = discordClient;
        _lavalink = lavalink;
        _lavalinkConfiguration = lavalinkConfiguration;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discordClient.MessageCreated += OnMessageCreated;
        _discordClient.VoiceStateUpdated += OnUserConnection;

        await _discordClient.ConnectAsync();
        _logger.LogInformation("Discord Client connected");
        await _lavalink.ConnectAsync(_lavalinkConfiguration);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _discordClient.DisconnectAsync();
        _discordClient.Dispose();
        _lavalink.Dispose();
        _logger.LogInformation("Discord Client disconnected");
    }

    private Task OnUserConnection(DiscordClient sender, VoiceStateUpdateEventArgs args)
    {
        using var scope = _scopeFactory.CreateScope();
        try
        {
            Thread.Sleep(500);
            _ = scope.ServiceProvider.GetRequiredService<IGrandEntranceService>().TriggerCustomEntranceScenarioAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An Exception occured in the OnUserConnection Event");
        }
        return Task.CompletedTask;
    }

    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
    {
        if (args.Message.Content.Equals("ping"))
            await args.Message.RespondAsync("Pong");
    }
}