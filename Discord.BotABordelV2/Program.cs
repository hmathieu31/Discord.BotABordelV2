using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services;
using Discord.BotABordelV2.Services.Media;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;

namespace Discord.BotABordelV2
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureServices((builder, services) =>
                {
                    services.AddHostedService<BotABordelService>()
                            .AddDiscordBotOptions(builder)
                            .AddDiscordClient()
                            .AddLavalink()
                            .AddTransient<StreamingMediaService>()
                            .AddTransient<LocalMediaService>()
                            .AddSingleton<IGrandEntranceService, GrandEntrancesService>();
                })
                .UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
                .RunConsoleAsync();

            await Log.CloseAndFlushAsync();
        }

        private static IServiceCollection AddDiscordClient(this IServiceCollection services)
        {
            services.AddSingleton((serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IOptions<DiscordBot>>().Value;
                var discordClient = new DiscordClient(new DiscordConfiguration
                {
                    Token = configuration.Token,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                    LoggerFactory = new LoggerFactory().AddSerilog(),
                    MinimumLogLevel = LogLevel.Trace,
                    LogUnknownEvents = true
                });
                var commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
                {
                    StringPrefixes = new[] { "!" }
                });
                var slash = discordClient.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = serviceProvider
                });

                discordClient.UseVoiceNext();
                commands.RegisterCommands(Assembly.GetExecutingAssembly());
                slash.RegisterCommands(Assembly.GetExecutingAssembly());

                return discordClient;
            });

            return services;
        }

        private static IServiceCollection AddLavalink(this IServiceCollection services)
        {
            services.AddSingleton((serviceProvider) =>
            {
                return serviceProvider.GetRequiredService<DiscordClient>().UseLavalink();
            });
            services.AddSingleton((serviceProvider) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<Lavalink>>().Value;
                var endpoint = new ConnectionEndpoint
                {
                    Hostname = options.Host, // From your server configuration.
                    Port = options.Port // From your server configuration
                };

                return new LavalinkConfiguration
                {
                    Password = options.Password, // From your server configuration.
                    RestEndpoint = endpoint,
                    SocketEndpoint = endpoint
                };
            });

            return services;
        }

        private static IServiceCollection AddDiscordBotOptions(this IServiceCollection services, HostBuilderContext context)
        {
            var botSection = context.Configuration.GetRequiredSection("DiscordBot");
            var lavalinkSection = context.Configuration.GetRequiredSection("Lavalink");

            services.Configure<DiscordBot>(botSection)
                    .Configure<Lavalink>(lavalinkSection);

            botSection.Bind(new DiscordBot());
            lavalinkSection.Bind(new Lavalink());

            return services;
        }
    }
}