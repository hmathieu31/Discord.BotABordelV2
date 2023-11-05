using Discord.BotABordelV2.Models;

namespace Discord.BotABordelV2.Interfaces;

/// <summary>
/// Interface for handling media playback in a voice channel.
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Pauses the player in the specified voice channel.
    /// </summary>
    /// <param name="channel">The voice channel to pause the player in.</param>
    /// <returns>The result of pausing the player.</returns>
    Task<PauseTrackResult> PauseTrackAsync(IVoiceChannel channel);

    /// <summary>
    /// Plays a track in the specified voice channel.
    /// </summary>
    /// <param name="track">The track to play.</param>
    /// <param name="channel">The voice channel to play the track in.</param>
    /// <returns>The result of playing the track.</returns>
    Task<PlayTrackResult> PlayTrackAsync(string track, IVoiceChannel channel);

    /// <summary>
    /// Resumes playback of a track in the specified voice channel.
    /// </summary>
    /// <param name="channel">The voice channel to resume playback in.</param>
    /// <returns>The result of resuming the track.</returns>
    Task<ResumeTrackResult> ResumeTrackAsync(IVoiceChannel channel);

    /// <summary>
    /// Skips the current track in the specified voice channel.
    /// </summary>
    /// <param name="channel">The voice channel to skip the track in.</param>
    /// <param name="user">The user that requested the skip.</param>
    /// <returns>The result of skipping the track.</returns>
    Task<SkipTrackResult> SkipTrackAsync(IVoiceChannel channel, IUser user);

    /// <summary>
    /// Stops the player in the specified voice channel.
    /// </summary>
    /// <param name="channel">The voice channel to stop the player in.</param>
    /// <returns>The result of stopping the player.</returns>
    Task<StopPlayerResult> StopPlayerAsync(IVoiceChannel channel);
}