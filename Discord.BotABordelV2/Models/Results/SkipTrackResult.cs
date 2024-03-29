﻿using Lavalink4NET.Players.Vote;
using Lavalink4NET.Tracks;

namespace Discord.BotABordelV2.Models.Results;

public enum SkipTrackStatus
{
    Skipped,
    VoteSubmitted,
    AlreadySubmitted,
    FinishedQueue,
    InternalException,
    NothingPlaying,
    UserNotInVoiceChannel,
    PlayerNotConnected
}

public class SkipTrackResult
{
    public SkipTrackResult(LavalinkTrack nextTrack)
    {
        NextTrack = nextTrack;
        Status = SkipTrackStatus.Skipped;
    }

    public SkipTrackResult(SkipTrackStatus status)
    {
        if (status is SkipTrackStatus.VoteSubmitted or SkipTrackStatus.AlreadySubmitted)
            throw new ArgumentException($"Use constructor with {nameof(VoteSkipInformation)} for this {nameof(status)}");

        Status = status;
    }

    public SkipTrackResult(VoteSkipInformation votesInfo, VoteSubmitStatus status)
    {
        VotesInfo = votesInfo;
        Status = (SkipTrackStatus)status;
    }

    public bool IsSuccess => Status is SkipTrackStatus.Skipped or SkipTrackStatus.FinishedQueue;

    public LavalinkTrack? NextTrack { get; private set; }

    public SkipTrackStatus Status { get; private set; }

    public VoteSkipInformation? VotesInfo { get; private set; }
}

public enum VoteSubmitStatus
{
    VoteSubmitted = SkipTrackStatus.VoteSubmitted,
    AlreadySubmitted = SkipTrackStatus.AlreadySubmitted
}