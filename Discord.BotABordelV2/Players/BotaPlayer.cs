using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Players.Vote;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Tracks;

namespace Discord.BotABordelV2.Players;

internal record SavedPlayingTrack(
    LavalinkTrack Track,
    TrackPosition Position
);

public sealed class BotaPlayer : VoteLavalinkPlayer
{
    public BotaPlayer(IPlayerProperties<BotaPlayer, VoteLavalinkPlayerOptions> properties) : base(properties)
    {
    }

    private LavalinkTrack? _eventTrack;

    private SavedPlayingTrack? _savedPlayingTrack;

    public static ValueTask<BotaPlayer> CreatePlayerAsync(IPlayerProperties<BotaPlayer, VoteLavalinkPlayerOptions> properties, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(properties);

        return ValueTask.FromResult(new BotaPlayer(properties));
    }

    public async Task PlayEventTrackAsync(LavalinkTrack track)
    {
        if (State == PlayerState.Playing)
        {
            _savedPlayingTrack = new(CurrentTrack!, Position!.Value);

            await PauseAsync();
        }

        _eventTrack = track;
        await PlayAsync(track, false);
    }

    protected override ValueTask NotifyTrackEndedAsync(ITrackQueueItem queueItem, TrackEndReason endReason, CancellationToken cancellationToken = default)
    {
        if (_eventTrack is not null && queueItem.Track == _eventTrack)
        {
            return HandleEventTrackEndedAsync();
        }

        return base.NotifyTrackEndedAsync(queueItem, endReason, cancellationToken);
    }

    private async ValueTask HandleEventTrackEndedAsync()
    {
        _eventTrack = null;

        if (_savedPlayingTrack is not null)
        {
            await PlayAsync(
                    _savedPlayingTrack.Track,
                    false,
                    new TrackPlayProperties
                    {
                        StartPosition = _savedPlayingTrack.Position.Position - TimeSpan.FromSeconds(0.5)
                    });

            _savedPlayingTrack = null;
        }
        else
        {
            await DisconnectAsync();
            await DisposeAsync();
        }
    }
}