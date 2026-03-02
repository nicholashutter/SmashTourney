export type BracketMode = "SINGLE_ELIMINATION" | "DOUBLE_ELIMINATION";

export type BracketLane = "WINNERS" | "LOSERS" | "GRAND_FINALS" | "GRAND_FINALS_RESET";

export type BracketMatchStatus = "PENDING" | "READY" | "IN_PROGRESS" | "COMPLETE";

export type CreateGameWithModeRequest = {
    bracketMode: BracketMode;
};

export type CreateGameWithModeResponse = {
    gameId?: string;
    GameId?: string;
    bracketMode?: BracketMode;
    BracketMode?: BracketMode;
};

export type BracketPlayerView = {
    playerId: string;
    displayName: string;
    seed: number;
    losses: number;
    eliminated: boolean;
};

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

export type BracketSnapshotResponse = {
    gameId: string;
    mode: BracketMode;
    gameStarted: boolean;
    isGrandFinalResetRequired: boolean;
    players: BracketPlayerView[];
    matches: BracketMatchView[];
};

export type CurrentMatchResponse = {
    gameId: string;
    matchId: string;
    lane: BracketLane;
    round: number;
    matchNumber: number;
    playerOneId: string;
    playerTwoId: string;
};

export type ReportMatchRequest = {
    matchId: string;
    winnerPlayerId: string;
};
