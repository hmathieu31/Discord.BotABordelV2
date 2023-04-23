using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.BotABordelV2.Services;
public class WideRatioService : IWideRatioService
{
    public WideRatioService(IMediaService mediaService,
                            LavalinkExtension lavalinkExtension,
                            IConfiguration configuration,
                            ILogger<WideRatioService> logger)
    {
        _mediaService = mediaService;
        _lavalinkExtension = lavalinkExtension;
        _logger = logger;
        _WIDE_RATIO_URL = configuration["WideRatio:TrackUrl"]
            ?? throw new InvalidOperationException("The track URL must be specified");
    }

    private const long _RATIO_ID = 202378979020111872;
    private readonly string _WIDE_RATIO_URL;
    private readonly IMediaService _mediaService;
    private readonly LavalinkExtension _lavalinkExtension;
    private readonly ILogger<WideRatioService> _logger;

    public bool ShouldTriggerWideRatioEvent(VoiceStateUpdateEventArgs args)
    {
        if (args.After.Channel is null) // If the user did not connected to a channel
            return false;

        if (args.After.Channel.Users.Count < 2) // If the channel is empty or there is only Aroty
            return false;

        if (args.User.Id != _RATIO_ID) // If the connecting user is not Aroty
            return false;

        return true;
    }

    public async Task TriggerWideRatioEventAsync(DSharpPlus.DiscordClient sender, VoiceStateUpdateEventArgs args)
    {
        if (args is null)
            throw new ArgumentNullException(nameof(args));

        if (ShouldTriggerWideRatioEvent(args))
        {
            var response = await _mediaService.PlayTrackAsync(sender.GetLavalink(), _WIDE_RATIO_URL, args.Channel);
            _logger.LogInformation("{response}", response ?? "Error getting the response from PlayTrack");
        }
    }
}
