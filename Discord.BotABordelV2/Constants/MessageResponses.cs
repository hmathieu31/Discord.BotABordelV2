using Discord.BotABordelV2.Models;
using System.Threading.Channels;

namespace Discord.BotABordelV2.Constants;

internal static class MessageResponses
{
    public const string UserNotConnected = "😖  Connect to a channel to stop the music";

    public const string InternalEx = "❗  An internal exception occured.";

    public const string NothingPlaying = "❓  Nothing seems to be playing at the moment";

    public const string NothingPaused = "⁉️  Player is not paused";
}