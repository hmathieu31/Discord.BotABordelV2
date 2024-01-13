namespace Discord.BotABordelV2.Constants;

internal static class MessageResponses
{
    public const string AlreadyVotedSkip = "‼️  You already voted to skip the track";
    public const string InternalEx = "❗  An internal exception occured.";
    public const string NoResults = "😖  No results.";
    public const string NothingPaused = "⁉️  Player is not paused";
    public const string NothingPlaying = "❓  Nothing seems to be playing at the moment";
    public const string PlayerStopped = "🛑  Stopped player and cleared queue";
    public const string PlayingTrackFormat = "🔈  Playing {0} ({1})";
    public const string QueuedTrackFormat = "🔈  Added to queue {0} ({1})";
    public const string SkippedFinishedQueue = "Skipped. Stopped playing because the queue is empty";
    public const string SkippedNowPlayingFormat = "Skipped.  🔈  Now playing {0} ({1})";
    public const string TrackPaused = "⏸️  Paused";
    public const string TrackResumed = "⏯️  Resumed";
    public const string UserNotConnected = "😖  Connect to a channel to stop the music";
    public const string VotedSkipFormat = "✅  Voted to skip the track. {0} % votes reached";
}