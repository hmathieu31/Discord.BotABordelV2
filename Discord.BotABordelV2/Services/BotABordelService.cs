using Discord.BotABordelV2.Interfaces;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2.Services;

public class BotABordelService : IHostedService
{
    private readonly ILogger<BotABordelService> _logger;
    private readonly DiscordClient _discordClient;
    private readonly IWideRatioService _wideRatioService;
    private readonly LavalinkExtension _lavalink;
    private readonly LavalinkConfiguration _lavalinkConfiguration;

    public BotABordelService(ILogger<BotABordelService> logger,
                             DiscordClient discordClient,
                             IWideRatioService wideRatioService,
                             LavalinkExtension lavalink,
                             LavalinkConfiguration lavalinkConfiguration)
    {
        _logger = logger;
        _discordClient = discordClient;
        _wideRatioService = wideRatioService;
        _lavalink = lavalink;
        _lavalinkConfiguration = lavalinkConfiguration;
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

    private async Task OnUserConnection(DiscordClient sender, VoiceStateUpdateEventArgs args)
    {
        Thread.Sleep(500);
        await _wideRatioService.TriggerWideRatioEventAsync(sender, args);
    }

    private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
    {
        if (args.Message.Content.Equals("ping"))
            await args.Message.RespondAsync("Pong");
    }
}