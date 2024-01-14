namespace Discord.BotABordelV2.Interfaces;

public interface IGrandEntranceService
{
    /// <summary>
    /// Triggers a custom entrance scenario for the connecting user and the next voice state.
    /// </summary>
    /// <param name="connectingUser">The user connecting to the voice channel.</param>
    /// <param name="nextVoiceState">The next voice state.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task TriggerCustomEntranceScenarioAsync(IUser connectingUser, IVoiceState nextVoiceState);
}