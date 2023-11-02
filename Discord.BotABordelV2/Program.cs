using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Extensions;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services;
using Discord.BotABordelV2.Services.Media;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

using Lavalink4NET.Extensions;

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
                    .AddLavalink(context)
                    .AddSingleton<StreamingMediaService>()
                    .AddSingleton<LocalMediaService>()
                    .AddSingleton<IGrandEntranceService, GrandEntrancesService>();
        });

        // Configure logging
        builder.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

        await builder.Build().RunAsync();

        await Log.CloseAndFlushAsync();
    }

#if !DEBUG
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
#endif

    /// <summary>
    /// Adds a Discord client to the service collection as Singleton.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    private static IServiceCollection AddDiscordClient(this IServiceCollection services)
    {
        //services.AddSingleton((serviceProvider) =>
        //{
        //    var configuration = serviceProvider.GetRequiredService<IOptions<DiscordBot>>().Value;
        //    var discordClient = new DiscordClient(new DiscordConfiguration
        //    {
        //        Token = configuration.Token,
        //        TokenType = TokenType.Bot,
        //        Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
        //        LoggerFactory = new LoggerFactory().AddSerilog(),
        //        MinimumLogLevel = LogLevel.Trace,
        //        LogUnknownEvents = true
        //    });
        //    var commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
        //    {
        //        StringPrefixes = new[] { "!" }
        //    });
        //    var slash = discordClient.UseSlashCommands(new SlashCommandsConfiguration
        //    {
        //        Services = serviceProvider
        //    });

        // discordClient.UseVoiceNext(); commands.RegisterCommands(Assembly.GetExecutingAssembly()); slash.RegisterCommands(Assembly.GetExecutingAssembly());

        //    return discordClient;
        //});

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
                       .ConfigureLavalink(config =>
                       {
                           var options = context.Configuration.GetRequiredSection("Lavalink");

                           config.BaseAddress = new UriBuilder(
                               "http",
                               options.Retrieve("Host"),
                               options.Retrieve<int>("Port")
                               ).Uri;
                           config.Passphrase = options.Retrieve("Password");
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
        var botSection = context.Configuration.GetRequiredSection("DiscordBot");
        var lavalinkSection = context.Configuration.GetRequiredSection("Lavalink");

        services.Configure<DiscordBot>(botSection)
                .Configure<Lavalink>(lavalinkSection);

        botSection.Bind(new DiscordBot());
        lavalinkSection.Bind(new Lavalink());

        return services;
    }
}