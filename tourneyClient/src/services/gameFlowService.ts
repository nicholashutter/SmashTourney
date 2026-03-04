import { normalizePlayers } from "@/lib/normalizePlayer";
import { BracketSnapshotResponse, CurrentMatchResponse, GameStateResponse } from "@/models/entities/Bracket";
import { Player } from "@/models/entities/Player";
import { RequestService } from "@/services/RequestService";

// Defines response shape returned by GetPlayersInGame route.
type GetPlayersInGameResponse = {
    currentPlayers?: Player[];
};

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
    const response = await RequestService<"getPlayersInGame", Record<string, never>, GetPlayersInGameResponse>("getPlayersInGame", {
        body: {},
        routeParams: { gameId }
    });

    return normalizePlayers(response.currentPlayers ?? []);
};

// Fetches the high-level game state and returns null on failure.
export const fetchGameState = async (gameId: string): Promise<GameStateResponse | null> =>
{
    try
    {
        return await RequestService<"getFlowState", never, GameStateResponse>("getFlowState", {
            routeParams: { gameId }
        });
    }
    catch
    {
        return null;
    }
};

// Fetches all data needed for bracket view rendering.
export const fetchBracketViewData = async (gameId: string): Promise<BracketViewData> =>
{
    let snapshot: BracketSnapshotResponse | null = null;
    let currentMatch: CurrentMatchResponse | null = null;

    try
    {
        snapshot = await RequestService<"getBracket", never, BracketSnapshotResponse>("getBracket", {
            routeParams: { gameId }
        });
    }
    catch
    {
        snapshot = null;
    }

    try
    {
        currentMatch = await RequestService<"getCurrentMatch", never, CurrentMatchResponse>("getCurrentMatch", {
            routeParams: { gameId }
        });
    }
    catch
    {
        currentMatch = null;
    }

    const gameState = await fetchGameState(gameId);

    return {
        snapshot,
        currentMatch,
        gameState
    };
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
