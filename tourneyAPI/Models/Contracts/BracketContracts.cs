namespace Contracts;

using Enums;

// Specifies options used when creating a new game.
public sealed record CreateGameOptions(
    BracketMode BracketMode = BracketMode.SINGLE_ELIMINATION
);

// Represents the winner selection for a reported bracket match.
public sealed record ReportMatchRequest(
    Guid MatchId,
    Guid WinnerPlayerId
);

// Represents a player's bracket state for UI rendering.
public sealed record BracketPlayerView(
    Guid PlayerId,
    string DisplayName,
    int Seed,
    int Losses,
    bool Eliminated
);

// Represents one bracket match and its progression links.
public sealed record BracketMatchView(
    Guid MatchId,
    BracketLane Lane,
    int Round,
    int MatchNumber,
    Guid? PlayerOneId,
    Guid? PlayerTwoId,
    Guid? WinnerId,
    BracketMatchStatus Status,
    Guid? NextMatchForWinner,
    Guid? NextMatchForLoser
);

// Represents a full bracket snapshot for a game.
public sealed record BracketSnapshotResponse(
    Guid GameId,
    BracketMode Mode,
    bool GameStarted,
    bool IsGrandFinalResetRequired,
    IReadOnlyList<BracketPlayerView> Players,
    IReadOnlyList<BracketMatchView> Matches
);

// Represents the currently active match in a game.
public sealed record CurrentMatchResponse(
    Guid GameId,
    Guid MatchId,
    BracketLane Lane,
    int Round,
    int MatchNumber,
    Guid PlayerOneId,
    Guid PlayerTwoId
);

// Represents the high-level game state returned to the client.
public sealed record GameStateResponse(
    Guid GameId,
    GameState State,
    bool GameStarted,
    Guid? CurrentMatchId,
    Guid? CurrentMatchPlayerOneId,
    Guid? CurrentMatchPlayerTwoId
);
