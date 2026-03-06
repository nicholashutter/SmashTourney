/* eslint-disable @typescript-eslint/no-explicit-any */

import { beforeEach, expect, test, vi } from "vitest";
import { Marth } from "../src/models/entities/Characters/Marth";
import { RequestService } from "../src/services/RequestService";
import { PersistentConnection } from "../src/services/PersistentConnection";

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

type FlowRunResult = {
    reportedMatches: number;
    autoResolvedByeMatches: number;
    totalMatches: number;
    finalState: string;
    finalMode: BracketMode;
    playersUpdatedRaised: boolean;
    gameStartedRaised: boolean;
    invokedHubMethods: string[];
};

const signalrEventHandlersByName = new Map<string, (...args: any[]) => void>();

const signalrOnEventSpy = vi.fn((eventName: string, handler: (...args: any[]) => void) =>
{
    signalrEventHandlersByName.set(eventName, handler);
});

const signalrOnReconnectedSpy = vi.fn();
const signalrInvokeMethodSpy = vi.fn().mockResolvedValue(undefined);
const signalrStartConnectionSpy = vi.fn().mockResolvedValue(undefined);
const signalrStopConnectionSpy = vi.fn().mockResolvedValue(undefined);

const simulatedSignalrConnection = {
    on: signalrOnEventSpy,
    onreconnected: signalrOnReconnectedSpy,
    invoke: signalrInvokeMethodSpy,
    start: signalrStartConnectionSpy,
    stop: signalrStopConnectionSpy,
    state: "Disconnected"
};

const signalrBuildConnectionSpy = vi.fn(() => simulatedSignalrConnection);
const signalrEnableReconnectSpy = vi.fn(() => ({ build: signalrBuildConnectionSpy }));
const signalrWithUrlSpy = vi.fn(() => ({ withAutomaticReconnect: signalrEnableReconnectSpy }));

vi.mock("@microsoft/signalr", async () =>
{
    const actualSignalrModule = await vi.importActual<typeof import("@microsoft/signalr")>("@microsoft/signalr");

    return {
        ...actualSignalrModule,
        HubConnectionBuilder: vi.fn(() => ({
            withUrl: signalrWithUrlSpy
        }))
    };
});

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
const isByePlayerId = (playerId: string): boolean =>
{
    return playerId.startsWith("bye-");
};

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

    const seededPlayerCount = state.seededPlayers.length;

    if (seededPlayerCount < 2)
    {
        return null;
    }

    while (state.resolvedMatches < state.totalMatches)
    {
        const firstIndex = state.resolvedMatches % seededPlayerCount;
        const secondIndex = (state.resolvedMatches + 1) % seededPlayerCount;
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
    const bracketSize = roundUpToPowerOfTwo(Math.max(2, playerCount));
    const state: SimulatorState = {
        gameId: `${mode.toLowerCase()}-${playerCount}-game-id`,
        mode,
        realPlayers: [],
        seededPlayers: [],
        bracketSize,
        gameStarted: false,
        authenticated: false,
        resolvedMatches: 0,
        reportedMatches: 0,
        autoResolvedByeMatches: 0,
        totalMatches: mode === "SINGLE_ELIMINATION" ? bracketSize - 1 : (bracketSize * 2) - 2,
        currentMatch: null
    };

    const handler = async (input: RequestInfo | URL, init?: RequestInit): Promise<MockResponse> =>
    {
        const rawUrl = typeof input === "string" ? input : input.toString();
        const parsedUrl = new URL(rawUrl);
        const path = parsedUrl.pathname;
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
            const flowState = !state.gameStarted
                ? "LOBBY_WAITING"
                : state.resolvedMatches >= state.totalMatches
                    ? "COMPLETE"
                    : "IN_MATCH_ACTIVE";

            return toJsonResponse(200, {
                gameId: state.gameId,
                state: flowState,
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
            const completedMatches = Array.from({ length: state.resolvedMatches }).map((_, index) => ({
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

            const pendingCount = Math.max(0, state.totalMatches - state.resolvedMatches);
            const pendingMatches = Array.from({ length: pendingCount }).map((_, index) => ({
                matchId: `${state.gameId}-pending-${index + 1}`,
                lane: "WINNERS",
                round: 1,
                matchNumber: state.resolvedMatches + index + 1,
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

const runFrontendLifecycle = async (mode: BracketMode, playerCount: number): Promise<FlowRunResult> =>
{
    const simulator = createFlowSimulator(mode, playerCount);
    global.fetch = vi.fn(simulator) as any;

    await RequestService("login", {
        body: {
            UserName: "dummy01",
            Password: "DummyPass!01"
        }
    });

    await RequestService("sessionStatus");

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

    const realtimeService = new PersistentConnection();
    let playersUpdatedRaised = false;
    let gameStartedRaised = false;

    realtimeService.setOnPlayersUpdated(() =>
    {
        playersUpdatedRaised = true;
    });

    realtimeService.setOnGameStarted(() =>
    {
        gameStartedRaised = true;
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
    signalrEventHandlersByName.get("PlayersUpdated")?.([]);

    await RequestService("startGame", {
        routeParams: { gameId }
    });

    await realtimeService.notifyGameStarted(gameId);
    signalrEventHandlersByName.get("GameStarted")?.(gameId);

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

    const finalBracket = await RequestService<
        "getBracket",
        never,
        { mode: BracketMode; matches: Array<{ status: string }> }
    >("getBracket", {
        routeParams: { gameId }
    });

    await realtimeService.disconnect();

    return {
        reportedMatches,
        autoResolvedByeMatches: simulator.__state.autoResolvedByeMatches,
        totalMatches: simulator.__state.totalMatches,
        finalState: finalFlowState.state,
        finalMode: finalBracket.mode,
        playersUpdatedRaised,
        gameStartedRaised,
        invokedHubMethods: signalrInvokeMethodSpy.mock.calls.map((call) => String(call[0]))
    };
};

beforeEach(() =>
{
    vi.clearAllMocks();
    signalrEventHandlersByName.clear();
    global.fetch = vi.fn() as any;
});

const powerOfTwoPlayerCounts = [2, 4, 8, 16, 32, 64, 128];
const oddPlayerCounts = [3, 5, 9];

// Verifies single-elimination reports expected match count for power-of-two brackets.
test.each(powerOfTwoPlayerCounts)(
    "single-elimination reports %i-1 matches for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycle("SINGLE_ELIMINATION", playerCount);

        expect(flowResult.reportedMatches).toBe(playerCount - 1);
    }
);

// Verifies double-elimination minimum bound for reported matches.
test.each(powerOfTwoPlayerCounts)(
    "double-elimination reported matches meet lower bound for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", playerCount);

        expect(flowResult.reportedMatches).toBeGreaterThanOrEqual(playerCount - 1);
    }
);

// Verifies double-elimination maximum bound for reported matches.
test.each(powerOfTwoPlayerCounts)(
    "double-elimination reported matches meet upper bound for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", playerCount);

        expect(flowResult.reportedMatches).toBeLessThanOrEqual((playerCount * 2) - 1);
    }
);

// Verifies odd-size single-elimination auto-resolves at least one bye match.
test.each(oddPlayerCounts)(
    "single-elimination odd bracket auto-resolves byes for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycle("SINGLE_ELIMINATION", playerCount);

        expect(flowResult.autoResolvedByeMatches).toBeGreaterThan(0);
    }
);

// Verifies odd-size single-elimination leaves fewer reported than total matches.
test.each(oddPlayerCounts)(
    "single-elimination odd bracket has unresolved-by-user matches for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycle("SINGLE_ELIMINATION", playerCount);

        expect(flowResult.reportedMatches).toBeLessThan(flowResult.totalMatches);
    }
);

// Verifies odd-size double-elimination auto-resolves at least one bye match.
test.each(oddPlayerCounts)(
    "double-elimination odd bracket auto-resolves byes for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", playerCount);

        expect(flowResult.autoResolvedByeMatches).toBeGreaterThan(0);
    }
);

// Verifies odd-size double-elimination leaves fewer reported than total matches.
test.each(oddPlayerCounts)(
    "double-elimination odd bracket has unresolved-by-user matches for %i players",
    async (playerCount) =>
    {
        const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", playerCount);

        expect(flowResult.reportedMatches).toBeLessThan(flowResult.totalMatches);
    }
);

// Verifies lifecycle reaches COMPLETE flow state.
test("lifecycle reaches COMPLETE state", async () =>
{
    const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", 8);

    expect(flowResult.finalState).toBe("COMPLETE");
});

// Verifies final bracket mode matches selected mode.
test("lifecycle returns selected bracket mode", async () =>
{
    const flowResult = await runFrontendLifecycle("SINGLE_ELIMINATION", 8);

    expect(flowResult.finalMode).toBe("SINGLE_ELIMINATION");
});

// Verifies PlayersUpdated callback is raised during lifecycle.
test("lifecycle raises PlayersUpdated callback", async () =>
{
    const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", 8);

    expect(flowResult.playersUpdatedRaised).toBe(true);
});

// Verifies GameStarted callback is raised during lifecycle.
test("lifecycle raises GameStarted callback", async () =>
{
    const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", 8);

    expect(flowResult.gameStartedRaised).toBe(true);
});

// Verifies lifecycle invokes JoinGameGroup hub method.
test("lifecycle invokes JoinGameGroup", async () =>
{
    const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", 8);

    expect(flowResult.invokedHubMethods.includes("JoinGameGroup")).toBe(true);
});

// Verifies lifecycle invokes UpdatePlayers hub method.
test("lifecycle invokes UpdatePlayers", async () =>
{
    const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", 8);

    expect(flowResult.invokedHubMethods.includes("UpdatePlayers")).toBe(true);
});

// Verifies lifecycle invokes NotifyGameStarted hub method.
test("lifecycle invokes NotifyGameStarted", async () =>
{
    const flowResult = await runFrontendLifecycle("DOUBLE_ELIMINATION", 8);

    expect(flowResult.invokedHubMethods.includes("NotifyGameStarted")).toBe(true);
});
