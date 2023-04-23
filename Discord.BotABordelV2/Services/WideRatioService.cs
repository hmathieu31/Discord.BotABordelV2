using Discord.BotABordelV2.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
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
    public WideRatioService(ILocalMediaService mediaService,
                            IConfiguration configuration,
                            ILogger<WideRatioService> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
        _configuration = configuration;
        _WIDE_RATIO_URL = configuration["WideRatio:TrackUrl"]
            ?? throw new InvalidOperationException("The track URL must be specified");
    }

    private const long _RATIO_ID = 202382364498722816;
    //private const long _RATIO_ID = 254728767799296001;
    private readonly string _WIDE_RATIO_URL;
    private readonly ILocalMediaService _mediaService;
    private readonly ILogger<WideRatioService> _logger;
    private readonly IConfiguration _configuration;

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

    public async Task TriggerWideRatioEventAsync(DiscordClient sender, VoiceStateUpdateEventArgs args)
    {
        if (args is null)
            throw new ArgumentNullException(nameof(args));

        var trackPath = _configuration["WideRatio:TrackFilePath"];
        if (trackPath is null)
            return;


        if (ShouldTriggerWideRatioEvent(args))
        {
            await _mediaService.PlayTrackAsync(trackPath, args.Channel);
            //_logger.LogInformation("{response}", response ?? "Error getting the response from PlayTrack");
        }
    }
}
