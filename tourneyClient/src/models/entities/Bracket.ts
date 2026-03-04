// Defines supported tournament bracket formats.
export type BracketMode = "SINGLE_ELIMINATION" | "DOUBLE_ELIMINATION";

// Defines the bracket lane where a match is played.
export type BracketLane = "WINNERS" | "LOSERS" | "GRAND_FINALS" | "GRAND_FINALS_RESET";

// Defines lifecycle states for a bracket match.
export type BracketMatchStatus = "PENDING" | "READY" | "IN_PROGRESS" | "COMPLETE";

// Defines request payload used to create a game with a selected mode.
export type CreateGameWithModeRequest = {
    bracketMode: BracketMode;
    totalPlayers: number;
};

// Defines response payload returned when a game is created.
export type CreateGameWithModeResponse = {
    gameId?: string;
    GameId?: string;
    bracketMode?: BracketMode;
    BracketMode?: BracketMode;
};

// Defines one player row in a bracket snapshot.
export type BracketPlayerView = {
    playerId: string;
    displayName: string;
    seed: number;
    losses: number;
    eliminated: boolean;
};

// Defines one match row in a bracket snapshot.
export type BracketMatchView = {
    matchId: string;
    lane: BracketLane;
    round: number;
    matchNumber: number;
    playerOneId?: string;
    playerTwoId?: string;
    winnerId?: string;
    status: BracketMatchStatus;
    nextMatchForWinner?: string;
    nextMatchForLoser?: string;
};

// Defines complete bracket data for one game.
export type BracketSnapshotResponse = {
    gameId: string;
    mode: BracketMode;
    gameStarted: boolean;
    isGrandFinalResetRequired: boolean;
    players: BracketPlayerView[];
    matches: BracketMatchView[];
};

// Defines the currently active match data.
export type CurrentMatchResponse = {
    gameId: string;
    matchId: string;
    lane: BracketLane;
    round: number;
    matchNumber: number;
    playerOneId: string;
    playerTwoId: string;
};

// Defines high-level game state values from the backend.
export type GameState = "LOBBY_WAITING" | "BRACKET_VIEW" | "IN_MATCH_ACTIVE" | "COMPLETE";

// Defines high-level game state response from the backend.
export type GameStateResponse = {
    gameId: string;
    state: GameState;
    gameStarted: boolean;
    currentMatchId?: string;
    currentMatchPlayerOneId?: string;
    currentMatchPlayerTwoId?: string;
};

// Defines vote-ledger statuses returned by submit-vote route.
export type SubmitMatchVoteStatus =
    | "PENDING"
    | "COMMITTED"
    | "DUPLICATE_VOTE"
    | "CONFLICT"
    | "MATCH_NOT_ACTIVE"
    | "INVALID_WINNER"
    | "VOTER_NOT_PARTICIPANT"
    | "GAME_NOT_FOUND"
    | "BRACKET_NOT_STARTED"
    | "APPLY_FAILED";

// Defines payload used to submit one participant vote for a match winner.
export type SubmitMatchVoteRequest = {
    matchId: string;
    winnerPlayerId: string;
};

// Defines response payload returned from vote-ledger submit route.
export type SubmitMatchVoteResponse = {
    gameId: string;
    matchId: string;
    status: SubmitMatchVoteStatus;
    voteCount: number;
    committedWinnerPlayerId?: string;
};
