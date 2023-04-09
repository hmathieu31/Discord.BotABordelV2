using Discord.BotABordelV2.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using System.Reflection;

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
                })
                .Build();

            host.Run();
        }
    }
}