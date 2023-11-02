using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Services.Media;

using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services;

public class GrandEntrancesService : IGrandEntranceService
{
    private const int _SCENARIO_TRIGGER_PERCENT_CHANCE = 50;

    private readonly IMediaService _localMediaService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GrandEntrancesService> _logger;

    public GrandEntrancesService(IServiceScopeFactory scopeFactory,
                            ILogger<GrandEntrancesService> logger,
                            LocalMediaService mediaService)
    {
        _localMediaService = mediaService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task TriggerCustomEntranceScenarioAsync(IUser connectingUser, IVoiceState nextVoiceState)
    {
        if (connectingUser is null)
            throw new ArgumentNullException(nameof(connectingUser));

        if (nextVoiceState is null)
            throw new ArgumentNullException(nameof(nextVoiceState));

        if (!ShouldCustomEntrancesScenarioTrigger(nextVoiceState))
            return;

        using var scope = _scopeFactory.CreateAsyncScope();
        var entranceOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DiscordBot>>().Value.EntrancesEvents;
        var eventForUser = entranceOptions.Find(evnOpt => evnOpt.UserId == connectingUser.Id);
        if (eventForUser is not null)
        {
            try
            {
                _logger.LogDebug("Playing entrance {entrance}", eventForUser.Name);
                _logger.LogDebug("Playing entrance {path} path", eventForUser.TrackFilePath);
                await _localMediaService.PlayTrackAsync(eventForUser.TrackFilePath, nextVoiceState.VoiceChannel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while triggering entrance scenario for {entrance}", eventForUser.Name);
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