using Discord.BotABordelV2.Interfaces;
using DSharpPlus.EventArgs;

namespace Discord.BotABordelV2.Services;

public class WideRatioService : IWideRatioService
{
    private readonly ulong _RATIO_ID;

    private readonly string _WIDE_RATIO_TRACK;

    private readonly ILocalMediaService _mediaService;

    private readonly ILogger<WideRatioService> _logger;

    public WideRatioService(IConfiguration configuration,
                                                ILogger<WideRatioService> logger,
                            ILocalMediaService mediaService)
    {
        _mediaService = mediaService;
        _logger = logger;
        _RATIO_ID = configuration.GetValue<ulong>("WideRatio:RatioId");
        _WIDE_RATIO_TRACK = configuration.GetValue<string>("WideRatio:TrackFilePath")
            ?? throw new InvalidOperationException("The Trackfile path must be defined in appsettings");
    }

    public bool ShouldTriggerWideRatioEvent(VoiceStateUpdateEventArgs args)
    {
        if (args.After.Channel is null) // If the user did not connected to a channel
            return false;

        if (args.After.Channel.Users.Count < 1) // If the channel is empty or there is only Aroty
            return false;

        if (args.User.Id != _RATIO_ID) // If the connecting user is not Aroty
            return false;

        return true;
    }

    public async Task TriggerWideRatioEventAsync(VoiceStateUpdateEventArgs args)
    {
        if (args is null)
            throw new ArgumentNullException(nameof(args));

        if (ShouldTriggerWideRatioEvent(args))
        {
            _logger.LogDebug("Starting Grand Entrace");
            await _mediaService.PlayTrackAsync(_WIDE_RATIO_TRACK, args.After.Channel);
        }
    }
}