import { beforeEach, expect, test, vi } from "vitest";

const persistentConnectionSpies = vi.hoisted(() =>
{
    return {
        createGameWithModeSpy: vi.fn(),
        addPlayerSpy: vi.fn(),
        startGameSpy: vi.fn(),
        getPlayersInGameSpy: vi.fn(),
        getFlowStateSpy: vi.fn(),
        getCurrentMatchSpy: vi.fn(),
        getBracketSpy: vi.fn(),
        submitMatchVoteSpy: vi.fn(),
        createPlayerConnectionSpy: vi.fn(),
        disconnectSpy: vi.fn()
    };
});

vi.mock("../src/services/PersistentConnection", () =>
{
    return {
        PersistentConnection: vi.fn().mockImplementation(() =>
        {
            return {
                createGameWithMode: persistentConnectionSpies.createGameWithModeSpy,
                addPlayer: persistentConnectionSpies.addPlayerSpy,
                startGame: persistentConnectionSpies.startGameSpy,
                getPlayersInGame: persistentConnectionSpies.getPlayersInGameSpy,
                getFlowState: persistentConnectionSpies.getFlowStateSpy,
                getCurrentMatch: persistentConnectionSpies.getCurrentMatchSpy,
                getBracket: persistentConnectionSpies.getBracketSpy,
                submitMatchVote: persistentConnectionSpies.submitMatchVoteSpy,
                createPlayerConnection: persistentConnectionSpies.createPlayerConnectionSpy,
                disconnect: persistentConnectionSpies.disconnectSpy,
                setOnPlayersUpdated: vi.fn(),
                setOnGameStarted: vi.fn(),
                setOnFlowStateUpdated: vi.fn(),
                setOnCurrentMatchUpdated: vi.fn(),
                setOnBracketUpdated: vi.fn(),
                setOnVoteSubmitted: vi.fn()
            };
        })
    };
});

import
    {
        addPlayerToGameRealtime,
        connectToGameRealtime,
        createGameWithModeRealtime,
        fetchRealtimeBracketViewData,
        getPlayersInGameRealtime,
        startGameRealtime,
        submitMatchVoteRealtime
    } from "../src/services/RealtimeGameService";

beforeEach(() =>
{
    vi.clearAllMocks();

    persistentConnectionSpies.createGameWithModeSpy.mockResolvedValue({ gameId: "game-1", bracketMode: "SINGLE_ELIMINATION" });
    persistentConnectionSpies.addPlayerSpy.mockResolvedValue(true);
    persistentConnectionSpies.startGameSpy.mockResolvedValue(true);
    persistentConnectionSpies.getPlayersInGameSpy.mockResolvedValue([]);
    persistentConnectionSpies.getFlowStateSpy.mockResolvedValue({ state: "IN_MATCH_ACTIVE" });
    persistentConnectionSpies.getCurrentMatchSpy.mockResolvedValue({ matchId: "match-1" });
    persistentConnectionSpies.getBracketSpy.mockResolvedValue({ players: [], matches: [] });
    persistentConnectionSpies.submitMatchVoteSpy.mockResolvedValue({ status: "COMMITTED" });
    persistentConnectionSpies.createPlayerConnectionSpy.mockResolvedValue(undefined);
    persistentConnectionSpies.disconnectSpy.mockResolvedValue(undefined);
});

// Verifies realtime game creation delegates to hub client.
test("createGameWithModeRealtime delegates to persistent connection", async () =>
{
    await createGameWithModeRealtime({
        bracketMode: "SINGLE_ELIMINATION",
        totalPlayers: 8
    });

    expect(persistentConnectionSpies.createGameWithModeSpy).toHaveBeenCalledTimes(1);
});

// Verifies realtime add-player delegates to hub client.
test("addPlayerToGameRealtime delegates to persistent connection", async () =>
{
    await addPlayerToGameRealtime("game-1", { Id: "player-1" });

    expect(persistentConnectionSpies.addPlayerSpy).toHaveBeenCalledWith("game-1", { Id: "player-1" });
});

// Verifies realtime start-game delegates to hub client.
test("startGameRealtime delegates to persistent connection", async () =>
{
    await startGameRealtime("game-1");

    expect(persistentConnectionSpies.startGameSpy).toHaveBeenCalledWith("game-1");
});

// Verifies realtime fetch normalizes current-match when flow is not active.
test("fetchRealtimeBracketViewData nulls current match when flow is not active", async () =>
{
    persistentConnectionSpies.getFlowStateSpy.mockResolvedValue({ state: "BRACKET_VIEW" });
    persistentConnectionSpies.getCurrentMatchSpy.mockResolvedValue({ matchId: "match-1" });

    const result = await fetchRealtimeBracketViewData("game-1");

    expect(result.currentMatch).toBeNull();
});

// Verifies connect helper delegates to persistent connection startup.
test("connectToGameRealtime calls createPlayerConnection", async () =>
{
    await connectToGameRealtime("game-1");

    expect(persistentConnectionSpies.createPlayerConnectionSpy).toHaveBeenCalledWith("game-1");
});

// Verifies realtime vote submission delegates to hub client.
test("submitMatchVoteRealtime delegates to persistent connection", async () =>
{
    await submitMatchVoteRealtime("game-1", {
        matchId: "match-1",
        winnerPlayerId: "player-1"
    });

    expect(persistentConnectionSpies.submitMatchVoteSpy).toHaveBeenCalledWith("game-1", {
        matchId: "match-1",
        winnerPlayerId: "player-1"
    });
});

// Verifies realtime players call delegates to hub client.
test("getPlayersInGameRealtime delegates to persistent connection", async () =>
{
    await getPlayersInGameRealtime("game-1");

    expect(persistentConnectionSpies.getPlayersInGameSpy).toHaveBeenCalledWith("game-1");
});
