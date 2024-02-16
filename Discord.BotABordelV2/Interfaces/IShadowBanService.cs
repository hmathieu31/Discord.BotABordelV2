using Discord.BotABordelV2.Models.Tracks;

namespace Discord.BotABordelV2.Interfaces;

public interface IShadowBanService
{
    /// <summary>
    /// Verify if a track is banned
    /// </summary>
    /// <param name="track"></param>
    /// <returns><see langword="true"/> if the track is banned</returns>
    bool IsTrackBanned(Track track);
}