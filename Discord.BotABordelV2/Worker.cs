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

        public Worker(ILogger<Worker> logger,
                      DiscordClient discordClient,
                      LavalinkConfiguration lavalinkConfiguration,
                      LavalinkExtension lavalink)
        {
            _logger = logger;
            _discordClient = discordClient;
            _lavalinkConfiguration = lavalinkConfiguration;
            _lavalink = lavalink;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient.MessageCreated += OnMessageCreated;


            await _discordClient.ConnectAsync();
            _logger.LogInformation("Discord Client connected");
            await _lavalink.ConnectAsync(_lavalinkConfiguration);
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