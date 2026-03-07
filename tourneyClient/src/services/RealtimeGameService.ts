import
    {
        BracketSnapshotResponse,
        CreateGameWithModeRequest,
        CreateGameWithModeResponse,
        CurrentMatchResponse,
        GameStateResponse,
        SubmitMatchVoteRequest,
        SubmitMatchVoteResponse
    } from "@/models/entities/Bracket";
import { Player } from "@/models/entities/Player";
import { normalizePlayers } from "@/lib/normalizePlayer";
import { PersistentConnection } from "@/services/PersistentConnection";

// Stores the singleton SignalR connection used for all game-management operations.
const gameConnection = new PersistentConnection();

// Defines combined bracket-view data used across bracket and in-match pages.
export type RealtimeBracketViewData = {
    snapshot: BracketSnapshotResponse | null;
    currentMatch: CurrentMatchResponse | null;
    gameState: GameStateResponse | null;
};

// Connects to the game hub and joins one game group.
export const connectToGameRealtime = async (gameId: string): Promise<void> =>
{
    await gameConnection.createPlayerConnection(gameId);
};

// Disconnects from the game hub.
export const disconnectFromGameRealtime = async (): Promise<void> =>
{
    await gameConnection.disconnect();
};

// Creates a game through SignalR and returns the typed response payload.
export const createGameWithModeRealtime = async (
    request: CreateGameWithModeRequest
): Promise<CreateGameWithModeResponse> =>
{
    return await gameConnection.createGameWithMode(request);
};

// Adds a player to one game through SignalR.
export const addPlayerToGameRealtime = async (gameId: string, payload: unknown): Promise<boolean> =>
{
    return await gameConnection.addPlayer(gameId, payload as Player);
};

// Starts a game through SignalR.
export const startGameRealtime = async (gameId: string): Promise<boolean> =>
{
    return await gameConnection.startGame(gameId);
};

// Retrieves players in one game through SignalR and normalizes their IDs.
export const getPlayersInGameRealtime = async (gameId: string): Promise<Player[]> =>
{
    const players = await gameConnection.getPlayersInGame(gameId);
    return normalizePlayers(players);
};

// Retrieves current high-level flow state through SignalR.
export const getFlowStateRealtime = async (gameId: string): Promise<GameStateResponse | null> =>
{
    return await gameConnection.getFlowState(gameId);
};

// Retrieves current active match through SignalR.
export const getCurrentMatchRealtime = async (gameId: string): Promise<CurrentMatchResponse | null> =>
{
    return await gameConnection.getCurrentMatch(gameId);
};

// Retrieves full bracket snapshot through SignalR.
export const getBracketRealtime = async (gameId: string): Promise<BracketSnapshotResponse | null> =>
{
    return await gameConnection.getBracket(gameId);
};

// Submits one match vote through SignalR.
export const submitMatchVoteRealtime = async (
    gameId: string,
    request: SubmitMatchVoteRequest
): Promise<SubmitMatchVoteResponse> =>
{
    return await gameConnection.submitMatchVote(gameId, request);
};

// Fetches all bracket-view payloads through SignalR.
export const fetchRealtimeBracketViewData = async (gameId: string): Promise<RealtimeBracketViewData> =>
{
    const [snapshot, currentMatch, gameState] = await Promise.all([
        getBracketRealtime(gameId),
        getCurrentMatchRealtime(gameId),
        getFlowStateRealtime(gameId)
    ]);

    const resolvedCurrentMatch = gameState?.state === "IN_MATCH_ACTIVE" ? currentMatch : null;

    return {
        snapshot,
        currentMatch: resolvedCurrentMatch,
        gameState
    };
};

// Sets callback executed when players-updated events are received.
export const onPlayersUpdatedRealtime = (callback: (players: Player[]) => void): void =>
{
    gameConnection.setOnPlayersUpdated(callback);
};

// Sets callback executed when game-started events are received.
export const onGameStartedRealtime = (callback: (gameId: string) => void): void =>
{
    gameConnection.setOnGameStarted(callback);
};

// Sets callback executed when flow-state-updated events are received.
export const onFlowStateUpdatedRealtime = (callback: (flowState: GameStateResponse | null) => void): void =>
{
    gameConnection.setOnFlowStateUpdated(callback);
};

// Sets callback executed when current-match-updated events are received.
export const onCurrentMatchUpdatedRealtime = (callback: (currentMatch: CurrentMatchResponse | null) => void): void =>
{
    gameConnection.setOnCurrentMatchUpdated(callback);
};

// Sets callback executed when bracket-updated events are received.
export const onBracketUpdatedRealtime = (callback: (snapshot: BracketSnapshotResponse | null) => void): void =>
{
    gameConnection.setOnBracketUpdated(callback);
};
