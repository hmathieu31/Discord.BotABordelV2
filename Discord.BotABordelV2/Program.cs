using Discord.BotABordelV2.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Discord.BotABordelV2
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((builder, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton((serviceProvider) =>
                    {
                        var discordClient = new DiscordClient(new DiscordConfiguration
                        {
                            Token = builder.Configuration["DiscordBot:Token"],
                            TokenType = TokenType.Bot,
                            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
                        });
                        var commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
                        {
                            StringPrefixes = new[] { "!" }
                        });
                        var slash = discordClient.UseSlashCommands();

                        commands.RegisterCommands(Assembly.GetExecutingAssembly());
                        slash.RegisterCommands(Assembly.GetExecutingAssembly());

                        return discordClient;
                    });
                    services.AddSingleton((serviceProvider) =>
                    {
                        return serviceProvider.GetRequiredService<DiscordClient>().UseLavalink();
                    });
                    services.AddSingleton((serviceProvider) =>
                    {
                        var endpoint = new ConnectionEndpoint
                        {
                            Hostname = "127.0.0.1", // From your server configuration.
                            Port = 2333 // From your server configuration
                        };

                        return new LavalinkConfiguration
                        {
                            Password = "youshallnotpass", // From your server configuration.
                            RestEndpoint = endpoint,
                            SocketEndpoint = endpoint
                        };
                    });
                })
                .Build();

            host.Run();
        }
    }
}