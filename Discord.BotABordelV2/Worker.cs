using Discord.BotABordelV2.Services;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;

namespace Discord.BotABordelV2
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DiscordClient _discordClient;
        private readonly LavalinkConfiguration _lavalinkConfiguration;
        private readonly LavalinkExtension _lavalink;
        private readonly IWideRatioService _wideRatioService;

        public Worker(ILogger<Worker> logger,
                      DiscordClient discordClient,
                      LavalinkConfiguration lavalinkConfiguration,
                      LavalinkExtension lavalink,
                      IWideRatioService wideRatioService)
        {
            _logger = logger;
            _discordClient = discordClient;
            _lavalinkConfiguration = lavalinkConfiguration;
            _lavalink = lavalink;
            _wideRatioService = wideRatioService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient.MessageCreated += OnMessageCreated;
            _discordClient.VoiceStateUpdated += OnUserConnection;


            await _discordClient.ConnectAsync();
            _logger.LogInformation("Discord Client connected");
            await _lavalink.ConnectAsync(_lavalinkConfiguration);
        }

        private async Task OnUserConnection(DiscordClient sender, VoiceStateUpdateEventArgs args)
        {
            await _wideRatioService.TriggerWideRatioEventAsync(sender, args);
        }

        private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Message.Content.Equals("ping"))
                await args.Message.RespondAsync("Pong");
        }


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordClient.DisconnectAsync();
            _discordClient.Dispose();
            _logger.LogInformation("Discord Client disconnected");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.CompletedTask;
    }
}