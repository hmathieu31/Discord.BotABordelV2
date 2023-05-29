using System.Diagnostics;
using System.Reflection;

using Azure.Identity;

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

namespace Discord.BotABordelV2;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
                          .UseConsoleLifetime();

#if !DEBUG
        // Add Azure App Configuration
        builder.UseAzureAppConfiguration();
#endif

        // Configure services
        builder.ConfigureServices((context, services) =>
        {
            services.AddHostedService<BotABordelService>()
                    .AddDiscordBotOptions(context)
                    .AddDiscordClient()
                    .AddLavalink()
                    .AddScoped<StreamingMediaService>()
                    .AddScoped<LocalMediaService>()
                    .AddScoped<IGrandEntranceService, GrandEntrancesService>();
        });

        // Configure logging
        builder.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

        await builder.Build().RunAsync();

        await Log.CloseAndFlushAsync();
    }

    /// <summary>
    /// Adds Azure App Configuration to the service collection.
    /// </summary>
    /// <param name="builder">The host builder object.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    private static IHostBuilder UseAzureAppConfiguration(this IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            var settings = cfg.Build();
            cfg.AddAzureAppConfiguration(options =>
            {
                var connectionString = settings.GetConnectionString("AppConfig");
                if (connectionString is not null)
                    options.Connect(connectionString)
                    .ConfigureRefresh(refreshOpt => 
                        refreshOpt.Register("DiscordBot:Sentinel", true));
                else
                    Console.WriteLine("Azure App Config was not configured");
            });
        });

        return builder;
    }

    /// <summary>
    /// Adds a Discord client to the service collection as Singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
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

    /// <summary>
    /// Adds Lavalink to the service collection as Singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
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

    /// <summary>
    /// Configure options for the Discord bot.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="context">The host builder context.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
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