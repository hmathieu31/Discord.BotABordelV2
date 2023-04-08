using DSharpPlus;

namespace Discord.BotABordelV2
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton((serviceProvider) =>
                    {
                        var discordClient = new DiscordClient(new DiscordConfiguration
                        {
                            Token = serviceProvider.GetRequiredService<IConfiguration>()["DiscordBot:Token"],
                            TokenType = TokenType.Bot,
                            Intents = DiscordIntents.AllUnprivileged
                        });
                        return discordClient;
                    });
                })
                .Build();

            host.Run();
        }
    }
}