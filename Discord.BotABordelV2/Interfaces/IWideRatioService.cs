namespace Discord.BotABordelV2.Interfaces;

public interface IWideRatioService
{
    bool ShouldTriggerWideRatioEvent(DSharpPlus.EventArgs.VoiceStateUpdateEventArgs args);

    Task TriggerWideRatioEventAsync(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.VoiceStateUpdateEventArgs args);
}