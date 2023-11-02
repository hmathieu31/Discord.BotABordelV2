namespace Discord.BotABordelV2.Exceptions;



[Serializable]
public class ConfigurationNotFoundException : Exception
{
    public ConfigurationNotFoundException() { }
    public ConfigurationNotFoundException(string key) : base($"Configuration with key '{key}' not found") { }
    public ConfigurationNotFoundException(string key, Exception inner) : base($"Configuration with key '{key}' not found", inner) { }
    protected ConfigurationNotFoundException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}