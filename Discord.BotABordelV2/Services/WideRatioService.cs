using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services;

public class WideRatioService : IWideRatioService
{
    private readonly ulong _RATIO_ID;

    private readonly string _WIDE_RATIO_TRACK;

    private readonly ILocalMediaService _mediaService;

    private readonly ILogger<WideRatioService> _logger;

    public WideRatioService(IOptions<DiscordBot> options,
                            ILogger<WideRatioService> logger,
                            ILocalMediaService mediaService)
    {
        _mediaService = mediaService;
        _logger = logger;
        _RATIO_ID = options.Value.EntrancesEvents.WideRatio.RatioId;
        _WIDE_RATIO_TRACK = options.Value.EntrancesEvents.WideRatio.TrackFilePath;
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