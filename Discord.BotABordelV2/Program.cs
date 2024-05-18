using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;

using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Extensions;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services;
using Discord.BotABordelV2.Services.Media;
using Discord.BotABordelV2.Services.Permissions;
using Discord.BotABordelV2.Services.ShadowBan;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;

using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.InactivityTracking.Trackers.Idle;
using Lavalink4NET.InactivityTracking.Trackers.Users;

using Microsoft.Extensions.Azure;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;


namespace Discord.BotABordelV2;

public static class Program
{
    public static bool IsProduction() => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Production";

    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
                          .UseConsoleLifetime();

        if (IsProduction())
        {
            // Add Azure App Configuration
            builder.UseAzureAppConfiguration();
        }

        // Configure services
        builder.ConfigureServices((context, services) =>
        {
            services.AddHostedService<BotABordelService>()
                    .AddDiscordBotOptions(context)
                    .AddDiscordClient()
                    .AddLavalink(context)
                    .AddSingleton<StandardMediaService>()
                    .AddSingleton<GrandEntranceMediaService>()
                    .AddSingleton<TrollMediaService>()
                    .AddSingleton<ISearchCacheService, SearchCacheService>()
                    .AddSingleton<IShadowBanService, ShadowBanService>()
                    .AddSingleton<IGrandEntranceService, GrandEntrancesService>()
                    .AddSingleton<IPermissionsService, PermissionsService>();

            var appInsConn = context.Configuration.GetConnectionString("AppInsights");
            services.AddOpenTelemetry()
                    .ConfigureResource(builder => builder.AddService("BotABordel"))
                    .WithTracing(builder => builder.AddConsoleExporter()
                                                   .AddAzureMonitorTraceExporter(cfg => cfg.ConnectionString = appInsConn))
                    .WithMetrics(builder => builder.AddConsoleExporter()
                                                   .AddAzureMonitorMetricExporter(cfg => cfg.ConnectionString = appInsConn));
        });

        // Configure logging
        if (IsProduction())
        {
            builder.ConfigureLogging((context, config) =>
            {
                config.AddSerilog();
                config.AddOpenTelemetry(logging =>
                {
                    logging.AddConsoleExporter()
                           .AddAzureMonitorLogExporter(cfg => cfg.ConnectionString = context.Configuration.GetConnectionString("AppInsights"));
                });
            });
        }
        else
        {
            builder.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
        }

        await builder.Build().RunAsync();

        await Log.CloseAndFlushAsync();
    }

    /// <summary>
    /// Adds Azure App Configuration to the service collection.
    /// </summary>
    /// <param name="builder">The host builder object.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used when not DEBUG")]
    private static IHostBuilder UseAzureAppConfiguration(this IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            cfg.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(Environment.GetEnvironmentVariable("AZURE_APPCONFIGURATION_ENDPOINT")
                    ?? throw new InvalidOperationException("Required config 'AZURE_APPCONFIGURATION_ENDPOINT' not found")),
                    CreateAzureCredentials());
            });
        });

        return builder;
    }

    private static DefaultAzureCredential CreateAzureCredentials() => new(new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_APPCONFIGURATION_CLIENTID")
    });

    /// <summary>
    /// Adds a Discord client to the service collection as Singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    private static IServiceCollection AddDiscordClient(this IServiceCollection services)
    {
        services.AddSingleton(services => new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
        })
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<InteractionService>();

        return services;
    }

    /// <summary>
    /// Adds Lavalink to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="context">The host builder context.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    private static IServiceCollection AddLavalink(this IServiceCollection services, HostBuilderContext context)
    {
        return services.AddLavalink()
                       .AddInactivityTracking()
                       .ConfigureLavalink(config =>
                       {
                           var options = context.Configuration.GetRequiredSection("Lavalink");

                           config.BaseAddress = new Uri(
                               $"{options.Retrieve("Scheme")}://{options.Retrieve("Host")}:{options.Retrieve<int>("Port")}"
                               );
                           config.WebSocketUri = new Uri($"ws://{options.Retrieve("Host")}:{options.Retrieve("Port")}/v4/websocket");
                           config.Passphrase = options.Retrieve("Password");
                           config.Label = "Node";
                       })
                       .Configure<IdleInactivityTrackerOptions>(config =>
                       {
                           config.Timeout = TimeSpan.FromMinutes(5);
                       })
                       .Configure<UsersInactivityTrackerOptions>(config =>
                       {
                           config.Timeout = TimeSpan.FromSeconds(30);
                       });
    }

    /// <summary>
    /// Configure options for the Discord bot.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="context">The host builder context.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    private static IServiceCollection AddDiscordBotOptions(this IServiceCollection services, HostBuilderContext context)
    {
        var config = context.Configuration;

        services.AddOptionsWithValidateOnStart<DiscordBotOptions>()
                .Bind(config.GetRequiredSection(DiscordBotOptions.SectionName));
        services.AddOptionsWithValidateOnStart<LavalinkOptions>()
                .Bind(config.GetRequiredSection(LavalinkOptions.SectionName));
        services.AddOptionsWithValidateOnStart<PermissionsOptions>()
                .Bind(config.GetRequiredSection(PermissionsOptions.SectionName));
        services.AddOptionsWithValidateOnStart<ShadowBanOptions>()
                .Bind(config.GetRequiredSection(ShadowBanOptions.SectionName));
        services.AddOptionsWithValidateOnStart<EmotesOptions>()
                .Bind(config.GetSection(EmotesOptions.SectionName));

        return services;
    }
}