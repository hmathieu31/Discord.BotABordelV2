﻿namespace Discord.BotABordelV2.Constants;

internal static class MessageResponses
{
    public const string AlreadyVotedSkip = "‼️  You already voted to skip the track";
    public const string InternalEx = "❗  An internal exception occurred.";
    public const string NoResults = "😖  No results.";
    public const string NothingPaused = "⁉️  Player is not paused";
    public const string NothingPlaying = "❓  Nothing seems to be playing at the moment";
    public const string PlayerStopped = "🛑  Stopped player and cleared queue";
    public const string PlayingTrackFormat = "  Playing {0} ({1})";

    public const string SeachTracksFormat = "   Found {0} tracks"; 
    public const string QueuedTrackFormat = "  Added to queue {0} ({1})";
    public const string SkippedFinishedQueue = "Skipped. Stopped playing because the queue is empty";
    public const string SkippedNowPlayingFormat = "Skipped.  🔈  Now playing {0} ({1})";
    public const string TrackPaused = "⏸️  Paused";
    public const string TrackResumed = "⏯️  Resumed";
    public const string Unauthorized = "⛔ You do not have permission to use this command";
    public const string UserNotConnected = "😖  Connect to a voice channel to execute this command";
    public const string VotedSkipFormat = "✅  Voted to skip the track. {0} % votes reached";
}