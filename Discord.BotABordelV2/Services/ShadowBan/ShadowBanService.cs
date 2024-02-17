using Discord.BotABordelV2.Configuration;
using Discord.BotABordelV2.Interfaces;
using Discord.BotABordelV2.Models.Tracks;

using Microsoft.Extensions.Options;

namespace Discord.BotABordelV2.Services.ShadowBan;

public class ShadowBanService(IOptionsMonitor<ShadowBanOptions> options) : IShadowBanService
{
    public bool IsTrackBanned(Track track)
    {
        var bannedKeyWords = options.CurrentValue.BannedKeywords;
        var bannedUris = options.CurrentValue.BannedUris;

        if (bannedKeyWords.Exists(x => track.Title.Contains(x, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (bannedUris.Exists(x => new Uri(x) == track.Uri))
        {
            return true;
        }

        // Special guard for unsufferable tracks
        if (track.Title.Contains("Cum Zone", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}