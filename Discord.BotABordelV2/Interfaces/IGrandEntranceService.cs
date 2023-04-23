namespace Discord.BotABordelV2.Interfaces;

public interface IGrandEntranceService
{
    /// <summary>
    /// Triggers the custom entrance scenario for a new connecting member if under valid conditions.
    /// </summary>
    /// <param name="args">The <see cref="DSharpPlus.EventArgs.VoiceStateUpdateEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    Task TriggerCustomEntranceScenarioAsync(DSharpPlus.EventArgs.VoiceStateUpdateEventArgs args);
}