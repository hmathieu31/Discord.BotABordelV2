namespace Discord.BotABordelV2.Models.Tracks;

public record Track(
    string Identifier,
    string Title,
    Uri? Uri
);