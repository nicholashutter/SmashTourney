namespace Contracts;

using Enums;

public sealed record CreateGameOptions(
    BracketMode BracketMode = BracketMode.SINGLE_ELIMINATION
);

public sealed record ReportMatchRequest(
    Guid MatchId,
    Guid WinnerPlayerId
);

public sealed record BracketPlayerView(
    Guid PlayerId,
    string DisplayName,
    int Seed,
    int Losses,
    bool Eliminated
);

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

public sealed record BracketSnapshotResponse(
    Guid GameId,
    BracketMode Mode,
    bool GameStarted,
    bool IsGrandFinalResetRequired,
    IReadOnlyList<BracketPlayerView> Players,
    IReadOnlyList<BracketMatchView> Matches
);

public sealed record CurrentMatchResponse(
    Guid GameId,
    Guid MatchId,
    BracketLane Lane,
    int Round,
    int MatchNumber,
    Guid PlayerOneId,
    Guid PlayerTwoId
);
