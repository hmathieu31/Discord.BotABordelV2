namespace Discord.BotABordelV2.Exceptions;

public class ConfigurationNotFoundException : Exception
{
    public ConfigurationNotFoundException()
    { }

    public ConfigurationNotFoundException(string key) : base($"Configuration with key '{key}' not found")
    {
    }

    public ConfigurationNotFoundException(string key, Exception inner) : base($"Configuration with key '{key}' not found", inner)
    {
    }
}