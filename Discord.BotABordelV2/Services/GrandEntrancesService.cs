using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services.Media;

using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services;

public class GrandEntrancesService(IServiceScopeFactory scopeFactory,
                        ILogger<GrandEntrancesService> logger,
                        GrandEntranceMediaService mediaService) : IGrandEntranceService
{
    private const int _SCENARIO_TRIGGER_PERCENT_CHANCE = 50;

    public async Task TriggerCustomEntranceScenarioAsync(IUser connectingUser, IVoiceState nextVoiceState)
    {
        ArgumentNullException.ThrowIfNull(connectingUser);
        ArgumentNullException.ThrowIfNull(nextVoiceState);

        if (!ShouldCustomEntrancesScenarioTrigger(nextVoiceState))
            return;

        using var scope = scopeFactory.CreateAsyncScope();
        var entranceOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DiscordBotOptions>>().Value.EntrancesEvents;
        var eventForUser = entranceOptions.Find(evnOpt => evnOpt.UserId == connectingUser.Id);
        if (eventForUser is not null)
        {
            try
            {
                logger.LogDebug("Playing entrance {entrance}", eventForUser.Name);
                logger.LogDebug("Playing entrance {path} path", eventForUser.TrackFilePath);
                await mediaService.PlayTrackAsync(eventForUser.TrackFilePath, nextVoiceState.VoiceChannel, Models.PlaySource.Local);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while triggering entrance scenario for {entrance}", eventForUser.Name);
            }
        }
    }

    private static bool ShouldCustomEntrancesScenarioTrigger(IVoiceState nextVoiceState)
    {
        if (nextVoiceState.VoiceChannel is null) // If the user did not connected to a channel
            return false;

        if (Random.Shared.Next(((int)Math.Ceiling(100.0 / _SCENARIO_TRIGGER_PERCENT_CHANCE))) != 1)
            return false;

        return true;
    }
}