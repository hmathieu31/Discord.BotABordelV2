namespace Discord.BotABordelV2.Models.Tracks;

public record TrackQueueItem(
    string Identifier,
    string Title,
    Uri? Uri,
    int Position
) : Track(Identifier, Title, Uri);