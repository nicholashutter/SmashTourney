import { beforeEach, expect, test, vi } from "vitest";
import { CharacterId } from "../src/models/Enums/CharacterId";
import { Marth } from "../src/models/entities/Characters/Marth";
import { Player } from "../src/models/entities/Player";
import { RequestService } from "../src/services/RequestService";

type MockFetchJsonResponse = {
    ok: boolean;
    json: () => Promise<unknown>;
};

type MockFetchTextResponse = {
    ok: boolean;
    status?: number;
    headers: {
        get: (key: string) => string | null;
    };
    text: () => Promise<string>;
};

type GetPlayersInGameResponse = {
    gameId: string;
    gameName: string;
    players: Array<{
        currentCharacter: {
            id: CharacterId;
        };
    }>;
};

const fetchSpy = () =>
{
    return fetch as unknown as ReturnType<typeof vi.fn>;
};

const queueJsonResponse = (payload: unknown) =>
{
    const response: MockFetchJsonResponse = {
        ok: true,
        json: async () => payload
    };

    fetchSpy().mockResolvedValueOnce(response);
};

const queueTextResponse = (payload: unknown, ok = true, status = 200) =>
{
    const response: MockFetchTextResponse = {
        ok,
        status,
        headers: {
            get: () => "application/json"
        },
        text: async () => JSON.stringify(payload)
    };

    fetchSpy().mockResolvedValueOnce(response);
};

const parseFetchBody = (callIndex: number) =>
{
    const call = fetchSpy().mock.calls[callIndex] as [string, RequestInit];

    return JSON.parse(String(call[1].body));
};

const getFetchCall = (callIndex: number) =>
{
    return fetchSpy().mock.calls[callIndex] as [string, RequestInit];
};

beforeEach(() =>
{
    global.fetch = vi.fn();
});

// Verifies addPlayers targets the AddPlayer route, uses POST with JSON body, and serializes the player payload.
test("addPlayers posts to AddPlayer route with serialized player array", async () =>
{
    const gameId = "game-abc123";
    const players: Player[] = [
        {
            Id: "player-one",
            displayName: "Player One",
            currentGameId: gameId,
            currentCharacter: Marth
        }
    ];

    queueJsonResponse({ message: `Players Added to Game ${gameId}` });

    const result = await RequestService("addPlayers", {
        routeParams: { gameId },
        body: players
    });

    const [url, init] = getFetchCall(0);
    const body = parseFetchBody(0);

    expect(url).toContain(`/Games/AddPlayer/${gameId}`);
    expect(init.method).toBe("POST");
    expect((init.headers as Record<string, string>)["Content-Type"]).toBe("application/json");
    expect(init.credentials).toBe("include");
    expect(Array.isArray(body)).toBe(true);
    expect(body[0].displayName).toBe("Player One");
    expect(body[0].currentCharacter.id).toBe(CharacterId.Marth);
    expect(body[0].userId).toBeUndefined();
    expect(result).toEqual({ message: `Players Added to Game ${gameId}` });
});

// Verifies getPlayersInGame posts to the GetPlayersInGame route and deserializes the player roster.
test("getPlayersInGame posts to GetPlayersInGame route and returns roster", async () =>
{
    const gameId = "game-xyz789";

    queueJsonResponse({
        gameId,
        gameName: "Test Game",
        players: [
            {
                displayName: "Sam",
                currentGameId: gameId,
                currentCharacter: Marth
            }
        ]
    });

    const result = await RequestService<"getPlayersInGame", never, GetPlayersInGameResponse>("getPlayersInGame", {
        routeParams: { gameId }
    });

    const [url, init] = getFetchCall(0);

    expect(url).toContain(`/Games/GetPlayersInGame/${gameId}`);
    expect(init.method).toBe("POST");
    expect(result.gameId).toBe(gameId);
    expect(result.gameName).toBe("Test Game");
    expect(result.players[0].currentCharacter.id).toBe(CharacterId.Marth);
});

// Verifies createGameWithMode posts the bracket mode and echoes the created game id.
test("createGameWithMode posts bracket mode and returns GameId", async () =>
{
    const gameId = "7f3ebf71-704e-4d34-bca9-eb2852e6f922";

    queueTextResponse({ GameId: gameId });

    const created = await RequestService<"createGameWithMode", { bracketMode: string }, { GameId: string }>("createGameWithMode", {
        body: { bracketMode: "SINGLE_ELIMINATION" }
    });

    const [url, init] = getFetchCall(0);

    expect(url).toContain("/Games/CreateGameWithMode");
    expect(init.method).toBe("POST");
    expect(init.credentials).toBe("include");
    expect(parseFetchBody(0)).toEqual({ bracketMode: "SINGLE_ELIMINATION" });
    expect(created.GameId).toBe(gameId);
});

// Verifies getFlowState targets the GetFlowState route and returns the in-match state payload.
test("getFlowState targets GetFlowState route and returns state", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";
    const flowState = {
        gameId,
        state: "IN_MATCH_ACTIVE",
        gameStarted: true,
        currentMatchId: "43db2e62-f1fc-4c67-81ca-3578886f3d34",
        currentMatchPlayerOneId: "8aa22920-a8ea-4db7-81d8-91773893ce2a",
        currentMatchPlayerTwoId: "12761655-f130-42fb-8378-6566adf08d90"
    };

    queueTextResponse(flowState);

    const result = await RequestService<"getFlowState", never, typeof flowState>("getFlowState", {
        routeParams: { gameId }
    });

    const [url] = getFetchCall(0);

    expect(url).toContain(`/Games/GetFlowState/${gameId}`);
    expect(result.state).toBe("IN_MATCH_ACTIVE");
});

// Verifies getBracket returns the bracket mode from the persisted snapshot.
test("getBracket targets GetBracket route and returns bracket mode", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";

    queueTextResponse({
        gameId,
        mode: "DOUBLE_ELIMINATION",
        gameStarted: true,
        isGrandFinalResetRequired: false,
        players: [],
        matches: []
    });

    const result = await RequestService<"getBracket", never, { mode: string }>("getBracket", {
        routeParams: { gameId }
    });

    const [url] = getFetchCall(0);

    expect(url).toContain(`/Games/GetBracket/${gameId}`);
    expect(result.mode).toBe("DOUBLE_ELIMINATION");
});

// Verifies getCurrentMatch returns the active match id from the current match endpoint.
test("getCurrentMatch targets GetCurrentMatch route and returns match id", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";
    const currentMatch = {
        gameId,
        matchId: "43db2e62-f1fc-4c67-81ca-3578886f3d34",
        lane: "WINNERS",
        round: 1,
        matchNumber: 1,
        playerOneId: "8aa22920-a8ea-4db7-81d8-91773893ce2a",
        playerTwoId: "12761655-f130-42fb-8378-6566adf08d90"
    };

    queueTextResponse(currentMatch);

    const result = await RequestService<"getCurrentMatch", never, typeof currentMatch>("getCurrentMatch", {
        routeParams: { gameId }
    });

    const [url] = getFetchCall(0);

    expect(url).toContain(`/Games/GetCurrentMatch/${gameId}`);
    expect(result.matchId).toBe(currentMatch.matchId);
});

// Verifies submitMatchVote posts the winner selection and reports the committed status with vote count.
test("submitMatchVote targets SubmitMatchVote route and reports committed status", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";
    const payload = {
        matchId: "43db2e62-f1fc-4c67-81ca-3578886f3d34",
        winnerPlayerId: "8aa22920-a8ea-4db7-81d8-91773893ce2a"
    };

    queueTextResponse({
        gameId,
        matchId: payload.matchId,
        status: "COMMITTED",
        voteCount: 2,
        committedWinnerPlayerId: payload.winnerPlayerId
    });

    const result = await RequestService<"submitMatchVote", { matchId: string; winnerPlayerId: string }, { status: string; voteCount: number }>("submitMatchVote", {
        routeParams: { gameId },
        body: payload
    });

    const [url] = getFetchCall(0);

    expect(url).toContain(`/Games/SubmitMatchVote/${gameId}`);
    expect(parseFetchBody(0)).toEqual(payload);
    expect(result.status).toBe("COMMITTED");
});

// Verifies submitMatchVote surfaces the PENDING status when only one vote has been recorded.
test("submitMatchVote reports PENDING status with vote count", async () =>
{
    const gameId = "8f29daff-adfa-49bb-a8c8-4e4f9fb58e3b";

    queueTextResponse({
        gameId,
        matchId: "4d4da6f2-c56e-4c34-8d99-1bb3f12ec0dc",
        status: "PENDING",
        voteCount: 1,
        committedWinnerPlayerId: null
    });

    const result = await RequestService<"submitMatchVote", { matchId: string; winnerPlayerId: string }, {
        status: string;
        voteCount: number;
    }>("submitMatchVote", {
        routeParams: { gameId },
        body: {
            matchId: "4d4da6f2-c56e-4c34-8d99-1bb3f12ec0dc",
            winnerPlayerId: "fd723edf-9302-42f7-af35-07b2c0604ffc"
        }
    });

    expect(result.status).toBe("PENDING");
    expect(result.voteCount).toBe(1);
});

// Verifies sessionStatus reads the /users/session endpoint and reports authentication state.
test("sessionStatus reads /users/session and reports authentication", async () =>
{
    queueTextResponse({ IsAuthenticated: true, UserName: "dummy01" });

    const session = await RequestService<"sessionStatus", never, { IsAuthenticated: boolean; UserName: string }>("sessionStatus");

    const [url, init] = getFetchCall(0);

    expect(url).toContain("/users/session");
    expect(init.credentials).toBe("include");
    expect(session.IsAuthenticated).toBe(true);
    expect(session.UserName).toBe("dummy01");
});

// Verifies sessionStatus throws when the backend rejects the request as unauthorized.
test("sessionStatus throws on unauthorized response", async () =>
{
    queueTextResponse({ message: "unauthorized" }, false, 401);

    await expect(
        RequestService<"sessionStatus", never, { IsAuthenticated: boolean }>("sessionStatus")
    ).rejects.toThrow("HTTP 401");
});
