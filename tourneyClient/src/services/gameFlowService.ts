import { normalizePlayers } from "@/lib/normalizePlayer";
import { BracketSnapshotResponse, CurrentMatchResponse, GameStateResponse } from "@/models/entities/Bracket";
import { Player } from "@/models/entities/Player";
import
    {
        fetchRealtimeBracketViewData,
        getFlowStateRealtime,
        getPlayersInGameRealtime
    } from "@/services/RealtimeGameService";

// Defines data required by the bracket page.
export type BracketViewData = {
    snapshot: BracketSnapshotResponse | null;
    currentMatch: CurrentMatchResponse | null;
    gameState: GameStateResponse | null;
};

// Defines data required by the in-match page.
export type InMatchViewData = BracketViewData & {
    gamePlayers: Player[];
};

// Fetches and normalizes players currently assigned to a game.
export const fetchPlayersInGame = async (gameId: string): Promise<Player[]> =>
{
    const players = await getPlayersInGameRealtime(gameId);
    return normalizePlayers(players);
};

// Fetches the high-level game state and returns null on failure.
export const fetchGameState = async (gameId: string): Promise<GameStateResponse | null> =>
{
    try
    {
        return await getFlowStateRealtime(gameId);
    }
    catch
    {
        return null;
    }
};

// Fetches all data needed for bracket view rendering.
export const fetchBracketViewData = async (gameId: string): Promise<BracketViewData> =>
{
    return await fetchRealtimeBracketViewData(gameId);
};

// Fetches all data needed for the in-match view.
export const fetchInMatchViewData = async (gameId: string): Promise<InMatchViewData> =>
{
    const bracketView = await fetchBracketViewData(gameId);

    let gamePlayers: Player[] = [];
    try
    {
        gamePlayers = await fetchPlayersInGame(gameId);
    }
    catch
    {
        gamePlayers = [];
    }

    return {
        ...bracketView,
        gamePlayers
    };
};
