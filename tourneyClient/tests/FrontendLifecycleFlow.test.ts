/* eslint-disable @typescript-eslint/no-explicit-any */

import { beforeEach, expect, test, vi } from "vitest";
import SignalRService from "../src/services/PersistentConnection";
import { RequestService } from "../src/services/RequestService";
import Marth from "../src/models/entities/Characters/Marth";

type BracketMode = "SINGLE_ELIMINATION" | "DOUBLE_ELIMINATION";

type MockResponse = {
    ok: boolean;
    status: number;
    headers: {
        get: (key: string) => string | null;
    };
    text: () => Promise<string>;
};

type SimulatedMatch = {
    matchId: string;
    playerOneId: string;
    playerTwoId: string;
};

type SimulatorState = {
    gameId: string;
    mode: BracketMode;
    realPlayers: string[];
    seededPlayers: string[];
    bracketSize: number;
    gameStarted: boolean;
    authenticated: boolean;
    resolvedMatches: number;
    reportedMatches: number;
    autoResolvedByeMatches: number;
    totalMatches: number;
    currentMatch: SimulatedMatch | null;
};

const roundUpToPowerOfTwo = (value: number): number =>
{
    let result = 1;
    while (result < value)
    {
        result *= 2;
    }

    return result;
};

// Identifies synthetic bye players in simulated tournament flows.
const isByePlayerId = (playerId: string): boolean => playerId.startsWith("bye-");

const signalEventHandlers = new Map<string, (...args: any[]) => void>();

const mockOn = vi.fn((eventName: string, handler: (...args: any[]) => void) =>
{
    signalEventHandlers.set(eventName, handler);
});

const mockOnReconnected = vi.fn();
const mockInvoke = vi.fn().mockResolvedValue(undefined);
const mockStart = vi.fn().mockResolvedValue(undefined);
const mockStop = vi.fn().mockResolvedValue(undefined);

const mockConnection = {
    on: mockOn,
    onreconnected: mockOnReconnected,
    invoke: mockInvoke,
    start: mockStart,
    stop: mockStop,
    state: "Disconnected"
};

const mockBuild = vi.fn(() => mockConnection);
const mockWithAutomaticReconnect = vi.fn(() => ({ build: mockBuild }));
const mockWithUrl = vi.fn(() => ({ withAutomaticReconnect: mockWithAutomaticReconnect }));

vi.mock("@microsoft/signalr", async () =>
{
    const actual = await vi.importActual<typeof import("@microsoft/signalr")>("@microsoft/signalr");

    return {
        ...actual,
        HubConnectionBuilder: vi.fn(() => ({
            withUrl: mockWithUrl
        }))
    };
});

const toJsonResponse = (status: number, payload: unknown): MockResponse =>
{
    return {
        ok: status >= 200 && status < 300,
        status,
        headers: {
            get: (key: string) =>
            {
                if (key.toLowerCase() === "content-type")
                {
                    return "application/json";
                }

                return null;
            }
        },
        text: async () => JSON.stringify(payload)
    };
};

// Produces the next player-votable match while auto-resolving bye-involved matches.
const buildCurrentMatch = (state: SimulatorState): SimulatedMatch | null =>
{
    if (!state.gameStarted || state.resolvedMatches >= state.totalMatches)
    {
        return null;
    }

    const playerCount = state.seededPlayers.length;
    if (playerCount < 2)
    {
        return null;
    }

    while (state.resolvedMatches < state.totalMatches)
    {
        const firstIndex = state.resolvedMatches % playerCount;
        const secondIndex = (state.resolvedMatches + 1) % playerCount;
        const playerOneId = state.seededPlayers[firstIndex];
        const playerTwoId = state.seededPlayers[secondIndex];

        if (isByePlayerId(playerOneId) || isByePlayerId(playerTwoId))
        {
            state.autoResolvedByeMatches += 1;
            state.resolvedMatches += 1;
            continue;
        }

        return {
            matchId: `${state.gameId}-match-${state.resolvedMatches + 1}`,
            playerOneId,
            playerTwoId
        };
    }

    return null;
};

// Simulates API behavior for full frontend tournament lifecycle validation.
const createFlowSimulator = (mode: BracketMode, playerCount: number) =>
{
    const state: SimulatorState = {
        gameId: `${mode.toLowerCase()}-${playerCount}-game-id`,
        mode,
        realPlayers: [],
        seededPlayers: [],
        bracketSize: roundUpToPowerOfTwo(Math.max(2, playerCount)),
        gameStarted: false,
        authenticated: false,
        resolvedMatches: 0,
        reportedMatches: 0,
        autoResolvedByeMatches: 0,
        totalMatches: mode === "SINGLE_ELIMINATION"
            ? roundUpToPowerOfTwo(Math.max(2, playerCount)) - 1
            : (roundUpToPowerOfTwo(Math.max(2, playerCount)) * 2) - 2,
        currentMatch: null
    };

    const handler = async (input: RequestInfo | URL, init?: RequestInit): Promise<MockResponse> =>
    {
        const urlString = typeof input === "string" ? input : input.toString();
        const url = new URL(urlString);
        const path = url.pathname;
        const method = (init?.method ?? "GET").toUpperCase();

        if (path === "/users/login" && method === "POST")
        {
            state.authenticated = true;
            return toJsonResponse(200, { Message: "Login successful" });
        }

        if (path === "/users/session" && method === "GET")
        {
            if (!state.authenticated)
            {
                return toJsonResponse(401, { message: "Unauthorized" });
            }

            return toJsonResponse(200, {
                IsAuthenticated: true,
                UserId: "session-user-id",
                UserName: "dummy01"
            });
        }

        if (path === "/Games/CreateGameWithMode" && method === "POST")
        {
            return toJsonResponse(200, {
                GameId: state.gameId,
                BracketMode: state.mode
            });
        }

        if (path === `/Games/AddPlayer/${state.gameId}` && method === "POST")
        {
            const bodyText = init?.body ? String(init.body) : "{}";
            const parsedBody = JSON.parse(bodyText);
            const playerId = parsedBody.Id ?? parsedBody.id ?? `player-${state.realPlayers.length + 1}`;
            state.realPlayers.push(playerId);

            return toJsonResponse(200, { added: true, playerId });
        }

        if (path === `/Games/StartGame/${state.gameId}` && method === "POST")
        {
            state.gameStarted = true;
            const byeCount = Math.max(0, state.bracketSize - state.realPlayers.length);
            state.seededPlayers = [
                ...state.realPlayers,
                ...Array.from({ length: byeCount }, (_, index) => `bye-${index + 1}`)
            ];
            state.currentMatch = buildCurrentMatch(state);
            return toJsonResponse(200, { started: true });
        }

        if (path === `/Games/GetFlowState/${state.gameId}` && method === "GET")
        {
            let currentState = "LOBBY_WAITING";
            if (state.gameStarted)
            {
                currentState = state.resolvedMatches >= state.totalMatches ? "COMPLETE" : "IN_MATCH_ACTIVE";
            }

            return toJsonResponse(200, {
                gameId: state.gameId,
                state: currentState,
                gameStarted: state.gameStarted,
                currentMatchId: state.currentMatch?.matchId,
                currentMatchPlayerOneId: state.currentMatch?.playerOneId,
                currentMatchPlayerTwoId: state.currentMatch?.playerTwoId
            });
        }

        if (path === `/Games/GetCurrentMatch/${state.gameId}` && method === "GET")
        {
            state.currentMatch = buildCurrentMatch(state);

            if (!state.currentMatch)
            {
                return toJsonResponse(404, { message: "No current match" });
            }

            return toJsonResponse(200, {
                gameId: state.gameId,
                matchId: state.currentMatch.matchId,
                lane: "WINNERS",
                round: Math.floor(state.reportedMatches / 2) + 1,
                matchNumber: state.reportedMatches + 1,
                playerOneId: state.currentMatch.playerOneId,
                playerTwoId: state.currentMatch.playerTwoId
            });
        }

        if (path === `/Games/SubmitMatchVote/${state.gameId}` && method === "POST")
        {
            const bodyText = init?.body ? String(init.body) : "{}";
            const parsedBody = JSON.parse(bodyText);

            if (!state.currentMatch)
            {
                return toJsonResponse(400, { message: "No active match" });
            }

            if (parsedBody.matchId !== state.currentMatch.matchId)
            {
                return toJsonResponse(400, { message: "Mismatched match id" });
            }

            state.reportedMatches += 1;
            state.resolvedMatches += 1;
            state.currentMatch = buildCurrentMatch(state);
            return toJsonResponse(200, {
                gameId: state.gameId,
                matchId: parsedBody.matchId,
                status: "COMMITTED",
                voteCount: 2,
                committedWinnerPlayerId: parsedBody.winnerPlayerId
            });
        }

        if (path === `/Games/GetBracket/${state.gameId}` && method === "GET")
        {
            const completeCount = state.resolvedMatches;
            const pendingCount = Math.max(0, state.totalMatches - state.resolvedMatches);

            const completedMatches = Array.from({ length: completeCount }).map((_, index) => ({
                matchId: `${state.gameId}-complete-${index + 1}`,
                lane: "WINNERS",
                round: 1,
                matchNumber: index + 1,
                playerOneId: state.seededPlayers[0] ?? "",
                playerTwoId: state.seededPlayers[1] ?? "",
                winnerId: state.seededPlayers[0] ?? "",
                status: "COMPLETE",
                nextMatchForWinner: undefined,
                nextMatchForLoser: undefined
            }));

            const pendingMatches = Array.from({ length: pendingCount }).map((_, index) => ({
                matchId: `${state.gameId}-pending-${index + 1}`,
                lane: "WINNERS",
                round: 1,
                matchNumber: completeCount + index + 1,
                playerOneId: state.seededPlayers[0] ?? "",
                playerTwoId: state.seededPlayers[1] ?? "",
                winnerId: undefined,
                status: "READY",
                nextMatchForWinner: undefined,
                nextMatchForLoser: undefined
            }));

            return toJsonResponse(200, {
                gameId: state.gameId,
                mode: state.mode,
                gameStarted: state.gameStarted,
                isGrandFinalResetRequired: false,
                players: state.seededPlayers.map((playerId, index) => ({
                    playerId,
                    displayName: isByePlayerId(playerId) ? `BYE ${index + 1}` : `Player ${index + 1}`,
                    seed: index + 1,
                    losses: 0,
                    eliminated: false
                })),
                matches: [...completedMatches, ...pendingMatches]
            });
        }

        return toJsonResponse(404, { message: `Unhandled route ${method} ${path}` });
    };

    return Object.assign(handler, { __state: state });
};

// Executes one full frontend lifecycle run and returns progression metrics.
const runFrontendLifecycleToCompletion = async (
    mode: BracketMode,
    playerCount: number
): Promise<{ reportedMatches: number; autoResolvedByeMatches: number; totalMatches: number }> =>
{
    const simulator = createFlowSimulator(mode, playerCount);
    global.fetch = vi.fn(simulator) as any;

    await RequestService("login", {
        body: {
            UserName: "dummy01",
            Password: "DummyPass!01"
        }
    });

    const session = await RequestService<"sessionStatus", never, { IsAuthenticated: boolean }>("sessionStatus");
    expect(session.IsAuthenticated).toBe(true);

    const createResult = await RequestService<
        "createGameWithMode",
        { bracketMode: BracketMode },
        { gameId?: string; GameId?: string }
    >("createGameWithMode", {
        body: {
            bracketMode: mode
        }
    });

    const gameId = createResult.gameId ?? createResult.GameId ?? "";
    expect(gameId).not.toBe("");

    const realtimeService = new SignalRService();
    let playersUpdatedWasRaised = false;
    let gameStartedWasRaised = false;

    realtimeService.setOnPlayersUpdated(() =>
    {
        playersUpdatedWasRaised = true;
    });

    realtimeService.setOnGameStarted(() =>
    {
        gameStartedWasRaised = true;
    });

    await realtimeService.createPlayerConnection(gameId);

    for (let index = 0; index < playerCount; index++)
    {
        await RequestService("addPlayers", {
            routeParams: { gameId },
            body: {
                Id: `player-${index + 1}`,
                displayName: `Player ${index + 1}`,
                currentScore: 0,
                currentRound: 0,
                currentCharacter: Marth,
                currentGameId: gameId
            }
        });
    }

    await realtimeService.updateOthers(gameId);
    signalEventHandlers.get("PlayersUpdated")?.([]);

    await RequestService("startGame", {
        routeParams: { gameId }
    });

    await realtimeService.notifyGameStarted(gameId);
    signalEventHandlers.get("GameStarted")?.(gameId);

    expect(playersUpdatedWasRaised).toBe(true);
    expect(gameStartedWasRaised).toBe(true);

    expect(mockInvoke).toHaveBeenCalledWith("JoinGameGroup", gameId);
    expect(mockInvoke).toHaveBeenCalledWith("UpdatePlayers", gameId);
    expect(mockInvoke).toHaveBeenCalledWith("NotifyGameStarted", gameId);

    const maxIterations = Math.max(32, simulator.__state.totalMatches * 4);

    let reportedMatches = 0;

    for (let iteration = 0; iteration < maxIterations; iteration++)
    {
        const flowState = await RequestService<"getFlowState", never, { state: string }>("getFlowState", {
            routeParams: { gameId }
        });

        if (flowState.state === "COMPLETE")
        {
            break;
        }

        const currentMatch = await RequestService<
            "getCurrentMatch",
            never,
            { matchId: string; playerOneId: string }
        >("getCurrentMatch", {
            routeParams: { gameId }
        });

        await RequestService("submitMatchVote", {
            routeParams: { gameId },
            body: {
                matchId: currentMatch.matchId,
                winnerPlayerId: currentMatch.playerOneId
            }
        });

        reportedMatches += 1;
    }

    const finalFlowState = await RequestService<"getFlowState", never, { state: string }>("getFlowState", {
        routeParams: { gameId }
    });

    expect(finalFlowState.state).toBe("COMPLETE");

    const finalBracket = await RequestService<
        "getBracket",
        never,
        { mode: BracketMode; matches: Array<{ status: string }> }
    >("getBracket", {
        routeParams: { gameId }
    });

    expect(finalBracket.mode).toBe(mode);
    expect(finalBracket.matches.some((match) => match.status === "COMPLETE")).toBe(true);

    await realtimeService.disconnect();

    return {
        reportedMatches,
        autoResolvedByeMatches: simulator.__state.autoResolvedByeMatches,
        totalMatches: simulator.__state.totalMatches
    };
};

beforeEach(() =>
{
    vi.clearAllMocks();
    signalEventHandlers.clear();
    global.fetch = vi.fn() as any;
});

const powerOfTwoPlayerCounts = [2, 4, 8, 16, 32, 64, 128];
const oddPlayerCounts = [3, 5, 9];

test.each(powerOfTwoPlayerCounts)(
    "Frontend auth/realtime/rest flow completes single-elimination tournament for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycleToCompletion("SINGLE_ELIMINATION", playerCount);
        expect(flowResult.reportedMatches).toBe(playerCount - 1);
    }
);

test.each(powerOfTwoPlayerCounts)(
    "Frontend auth/realtime/rest flow completes double-elimination tournament for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycleToCompletion("DOUBLE_ELIMINATION", playerCount);
        expect(flowResult.reportedMatches).toBeGreaterThanOrEqual(playerCount - 1);
        expect(flowResult.reportedMatches).toBeLessThanOrEqual((playerCount * 2) - 1);
    }
);

test.each(oddPlayerCounts)(
    "Frontend flow auto-resolves bye matches for single-elimination odd bracket size %i",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycleToCompletion("SINGLE_ELIMINATION", playerCount);
        expect(flowResult.autoResolvedByeMatches).toBeGreaterThan(0);
        expect(flowResult.reportedMatches).toBeLessThan(flowResult.totalMatches);
    }
);

test.each(oddPlayerCounts)(
    "Frontend flow auto-resolves bye matches for double-elimination odd bracket size %i",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycleToCompletion("DOUBLE_ELIMINATION", playerCount);
        expect(flowResult.autoResolvedByeMatches).toBeGreaterThan(0);
        expect(flowResult.reportedMatches).toBeLessThan(flowResult.totalMatches);
    }
);
