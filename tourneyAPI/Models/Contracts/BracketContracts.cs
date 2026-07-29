namespace Contracts;

using Enums;

// Specifies options used when creating a new game.
public sealed record CreateGameOptions(
    BracketMode BracketMode = BracketMode.SINGLE_ELIMINATION,
    int TotalPlayers = 0
);

// Describes a returning player's place in a game and where they belong now.
//
// A player who drops has lost only what their browser tab was holding. Their
// identity cookie outlives the tab and their player row is already stored
// against the game, so given the game id from the URL the server can rebuild
// the whole session. This is what it answers with.
public sealed record PlayerSessionResponse(
    Guid GameId,
    Guid PlayerId,
    string DisplayName,
    bool IsHost,
    GameState State,
    bool GameStarted
);

// Describes one tournament as it appears in the games browser.
//
// Deliberately small. The browser exists so somebody on a phone can find the
// room they are meant to be in, and that decision needs a handful of facts:
// how big it is, whether it has started, and whether they already belong to it.
// Sending the full player roster with characters for every game on the server
// would be several kilobytes per row to answer a question nobody asked yet —
// the game screens fetch that once a game has actually been chosen.
public sealed record GameSummaryResponse(
    Guid GameId,
    BracketMode BracketMode,
    int PlayerCount,
    GameState State,
    DateTime CreatedUtc,
    bool IsHost,
    bool HasJoined
);

// Describes the outcome of a request to end a game.
//
// NOT_HOST is separate from GAME_NOT_FOUND on purpose: ending someone else's
// tournament is a refusal, not a missing thing, and the client says different
// words for each.
public enum EndGameStatus
{
    ENDED,
    GAME_NOT_FOUND,
    NOT_HOST
}

// Represents the winner selection for a reported bracket match.
public sealed record ReportMatchRequest(
    Guid MatchId,
    Guid WinnerPlayerId
);

// Represents one player's vote for a match winner.
public sealed record SubmitMatchVoteRequest(
    Guid MatchId,
    Guid WinnerPlayerId
);

// Describes the result of processing a submitted match vote.
public enum SubmitMatchVoteStatus
{
    PENDING,
    COMMITTED,
    DUPLICATE_VOTE,
    CONFLICT,
    MATCH_NOT_ACTIVE,
    INVALID_WINNER,
    VOTER_NOT_PARTICIPANT,
    GAME_NOT_FOUND,
    BRACKET_NOT_STARTED,
    APPLY_FAILED
}

// Represents the API payload returned after vote submission.
public sealed record SubmitMatchVoteResponse(
    Guid GameId,
    Guid MatchId,
    SubmitMatchVoteStatus Status,
    int VoteCount,
    Guid? CommittedWinnerPlayerId
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
