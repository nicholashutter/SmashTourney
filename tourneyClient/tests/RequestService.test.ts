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

// Verifies addPlayers returns success payload from backend.
test("addPlayers returns success payload", async () =>
{
    const gameId = "game-abc123";
    const players: Player[] = [
        {
            Id: "player-one",
            displayName: "Player One",
            currentScore: 0,
            currentRound: 1,
            currentGameId: gameId,
            currentCharacter: Marth
        }
    ];

    queueJsonResponse({ message: `Players Added to Game ${gameId}` });

    const result = await RequestService("addPlayers", {
        routeParams: { gameId },
        body: players
    });

    expect(result).toEqual({ message: `Players Added to Game ${gameId}` });
});

// Verifies addPlayers targets expected route path.
test("addPlayers calls AddPlayer route", async () =>
{
    const gameId = "game-abc123";

    queueJsonResponse({ message: "ok" });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: []
    });

    expect(getFetchCall(0)[0]).toContain(`/Games/AddPlayer/${gameId}`);
});

// Verifies addPlayers uses POST method.
test("addPlayers uses POST method", async () =>
{
    const gameId = "game-abc123";

    queueJsonResponse({ message: "ok" });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: []
    });

    expect(getFetchCall(0)[1].method).toBe("POST");
});

// Verifies addPlayers sets JSON content-type header.
test("addPlayers sets JSON content type", async () =>
{
    const gameId = "game-abc123";

    queueJsonResponse({ message: "ok" });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: []
    });

    expect((getFetchCall(0)[1].headers as Record<string, string>)["Content-Type"]).toBe("application/json");
});

// Verifies addPlayers serializes body as array.
test("addPlayers serializes array payload", async () =>
{
    const gameId = "game-abc123";

    queueJsonResponse({ message: "ok" });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: [{ Id: "player-one", displayName: "Player One", currentScore: 0, currentRound: 1, currentGameId: gameId, currentCharacter: Marth }]
    });

    expect(Array.isArray(parseFetchBody(0))).toBe(true);
});

// Verifies addPlayers serializes first player's displayName.
test("addPlayers serializes displayName", async () =>
{
    const gameId = "game-abc123";

    queueJsonResponse({ message: "ok" });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: [{ Id: "player-one", displayName: "Player One", currentScore: 0, currentRound: 1, currentGameId: gameId, currentCharacter: Marth }]
    });

    expect(parseFetchBody(0)[0].displayName).toBe("Player One");
});

// Verifies addPlayers serializes nested character id.
test("addPlayers serializes character id", async () =>
{
    const gameId = "game-abc123";

    queueJsonResponse({ message: "ok" });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: [{ Id: "player-one", displayName: "Player One", currentScore: 0, currentRound: 1, currentGameId: gameId, currentCharacter: Marth }]
    });

    expect(parseFetchBody(0)[0].currentCharacter.id).toBe(CharacterId.Marth);
});

// Verifies addPlayers payload may omit userId.
test("addPlayers allows payload without userId", async () =>
{
    const gameId = "f9c2d4eb-8786-449a-ae28-7823813339b6";

    queueJsonResponse({ success: true });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: {
            Id: "93f00506-bcc6-4d0d-a5f0-c588505f3b60",
            displayName: "NoUserIdClientPayload",
            currentScore: 0,
            currentRound: 0,
            currentCharacter: Marth,
            currentGameId: gameId
        }
    });

    expect(parseFetchBody(0).userId).toBeUndefined();
});

// Verifies getPlayersInGame response includes requested game id.
test("getPlayersInGame returns game id", async () =>
{
    const gameId = "game-xyz789";

    queueJsonResponse({
        gameId,
        gameName: "Test Game",
        players: [
            {
                displayName: "Sam",
                currentScore: 88,
                currentRound: 5,
                currentGameId: gameId,
                currentCharacter: Marth
            }
        ]
    });

    const result = await RequestService<"getPlayersInGame", never, GetPlayersInGameResponse>("getPlayersInGame", {
        routeParams: { gameId }
    });

    expect(result.gameId).toBe(gameId);
});

// Verifies getPlayersInGame response includes game name.
test("getPlayersInGame returns game name", async () =>
{
    const gameId = "game-xyz789";

    queueJsonResponse({
        gameId,
        gameName: "Test Game",
        players: []
    });

    const result = await RequestService<"getPlayersInGame", never, GetPlayersInGameResponse>("getPlayersInGame", {
        routeParams: { gameId }
    });

    expect(result.gameName).toBe("Test Game");
});

// Verifies getPlayersInGame returns player list.
test("getPlayersInGame returns players array", async () =>
{
    const gameId = "game-xyz789";

    queueJsonResponse({
        gameId,
        gameName: "Test Game",
        players: [
            {
                displayName: "Sam",
                currentScore: 88,
                currentRound: 5,
                currentGameId: gameId,
                currentCharacter: Marth
            }
        ]
    });

    const result = await RequestService<"getPlayersInGame", never, GetPlayersInGameResponse>("getPlayersInGame", {
        routeParams: { gameId }
    });

    expect(Array.isArray(result.players)).toBe(true);
});

// Verifies getPlayersInGame deserializes nested character id.
test("getPlayersInGame returns character id", async () =>
{
    const gameId = "game-xyz789";

    queueJsonResponse({
        gameId,
        gameName: "Test Game",
        players: [
            {
                displayName: "Sam",
                currentScore: 88,
                currentRound: 5,
                currentGameId: gameId,
                currentCharacter: Marth
            }
        ]
    });

    const result = await RequestService<"getPlayersInGame", never, GetPlayersInGameResponse>("getPlayersInGame", {
        routeParams: { gameId }
    });

    expect(result.players[0].currentCharacter.id).toBe(CharacterId.Marth);
});

// Verifies getPlayersInGame calls expected route path.
test("getPlayersInGame calls route with gameId", async () =>
{
    const gameId = "game-xyz789";

    queueJsonResponse({ gameId, gameName: "Test Game", players: [] });

    await RequestService("getPlayersInGame", {
        routeParams: { gameId },
        body: {}
    });

    expect(getFetchCall(0)[0]).toContain(`/Games/GetPlayersInGame/${gameId}`);
});

// Verifies getPlayersInGame uses POST method.
test("getPlayersInGame uses POST method", async () =>
{
    const gameId = "game-xyz789";

    queueJsonResponse({ gameId, gameName: "Test Game", players: [] });

    await RequestService("getPlayersInGame", {
        routeParams: { gameId },
        body: {}
    });

    expect(getFetchCall(0)[1].method).toBe("POST");
});

// Verifies createGameWithMode returns generated game id.
test("createGameWithMode returns GameId", async () =>
{
    const gameId = "7f3ebf71-704e-4d34-bca9-eb2852e6f922";

    queueTextResponse({ GameId: gameId });

    const created = await RequestService<"createGameWithMode", { bracketMode: string }, { GameId: string }>("createGameWithMode", {
        body: { bracketMode: "SINGLE_ELIMINATION" }
    });

    expect(created.GameId).toBe(gameId);
});

// Verifies sessionStatus throws when backend returns 401.
test("sessionStatus throws on unauthorized response", async () =>
{
    queueTextResponse({ message: "unauthorized" }, false, 401);

    await expect(
        RequestService<"sessionStatus", never, { IsAuthenticated: boolean }>("sessionStatus")
    ).rejects.toThrow("HTTP 401");
});

// Verifies createGameWithMode endpoint path.
test("createGameWithMode calls expected route", async () =>
{
    queueTextResponse({ GameId: "game-id" });

    await RequestService("createGameWithMode", {
        body: { bracketMode: "SINGLE_ELIMINATION" }
    });

    expect(getFetchCall(0)[0]).toContain("/Games/CreateGameWithMode");
});

// Verifies sessionStatus endpoint path.
test("sessionStatus calls expected route", async () =>
{
    queueTextResponse({ IsAuthenticated: true });

    await RequestService("sessionStatus");

    expect(getFetchCall(0)[0]).toContain("/users/session");
});

// Verifies createGameWithMode includes credentials.
test("createGameWithMode includes credentials", async () =>
{
    queueTextResponse({ GameId: "game-id" });

    await RequestService("createGameWithMode", {
        body: { bracketMode: "SINGLE_ELIMINATION" }
    });

    expect(getFetchCall(0)[1].credentials).toBe("include");
});

// Verifies sessionStatus includes credentials.
test("sessionStatus includes credentials", async () =>
{
    queueTextResponse({ IsAuthenticated: true });

    await RequestService("sessionStatus");

    expect(getFetchCall(0)[1].credentials).toBe("include");
});

// Verifies sessionStatus returns authenticated flag.
test("sessionStatus returns authentication state", async () =>
{
    queueTextResponse({ IsAuthenticated: true, UserName: "dummy01" });

    const session = await RequestService<"sessionStatus", never, { IsAuthenticated: boolean; UserName: string }>("sessionStatus");

    expect(session.IsAuthenticated).toBe(true);
});

// Verifies sessionStatus returns username.
test("sessionStatus returns username", async () =>
{
    queueTextResponse({ IsAuthenticated: true, UserName: "dummy01" });

    const session = await RequestService<"sessionStatus", never, { IsAuthenticated: boolean; UserName: string }>("sessionStatus");

    expect(session.UserName).toBe("dummy01");
});

// Verifies addPlayers call succeeds with authenticated flow payload.
test("authenticated addPlayers returns success", async () =>
{
    const gameId = "7f3ebf71-704e-4d34-bca9-eb2852e6f922";

    queueTextResponse({ success: true });

    const addResult = await RequestService("addPlayers", {
        routeParams: { gameId },
        body: {
            Id: "0de2acbb-b6f0-4f01-ab0a-3f7fb58d3f57",
            displayName: "AuthedUser",
            currentScore: 0,
            currentRound: 0,
            currentCharacter: Marth,
            currentGameId: gameId
        }
    });

    expect(addResult).toEqual({ success: true });
});

// Verifies addPlayers includes credentials for authenticated flow.
test("addPlayers includes credentials", async () =>
{
    const gameId = "7f3ebf71-704e-4d34-bca9-eb2852e6f922";

    queueTextResponse({ success: true });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: {
            Id: "0de2acbb-b6f0-4f01-ab0a-3f7fb58d3f57",
            displayName: "AuthedUser",
            currentScore: 0,
            currentRound: 0,
            currentCharacter: Marth,
            currentGameId: gameId
        }
    });

    expect(getFetchCall(0)[1].credentials).toBe("include");
});

// Verifies createGameWithMode echoes game id.
test("createGameWithMode returns gameId", async () =>
{
    queueTextResponse({ gameId: "mode-game-id", bracketMode: "DOUBLE_ELIMINATION" });

    const result = await RequestService<"createGameWithMode", { bracketMode: string }, { gameId: string; bracketMode: string }>(
        "createGameWithMode",
        {
            body: { bracketMode: "DOUBLE_ELIMINATION" }
        }
    );

    expect(result.gameId).toBe("mode-game-id");
});

// Verifies createGameWithMode echoes bracket mode.
test("createGameWithMode returns bracket mode", async () =>
{
    queueTextResponse({ gameId: "mode-game-id", bracketMode: "DOUBLE_ELIMINATION" });

    const result = await RequestService<"createGameWithMode", { bracketMode: string }, { gameId: string; bracketMode: string }>(
        "createGameWithMode",
        {
            body: { bracketMode: "DOUBLE_ELIMINATION" }
        }
    );

    expect(result.bracketMode).toBe("DOUBLE_ELIMINATION");
});

// Verifies createGameWithMode uses POST method.
test("createGameWithMode uses POST method", async () =>
{
    queueTextResponse({ gameId: "mode-game-id", bracketMode: "DOUBLE_ELIMINATION" });

    await RequestService("createGameWithMode", {
        body: { bracketMode: "DOUBLE_ELIMINATION" }
    });

    expect(getFetchCall(0)[1].method).toBe("POST");
});

// Verifies createGameWithMode serializes bracket payload.
test("createGameWithMode serializes request payload", async () =>
{
    queueTextResponse({ gameId: "mode-game-id", bracketMode: "DOUBLE_ELIMINATION" });

    await RequestService("createGameWithMode", {
        body: { bracketMode: "DOUBLE_ELIMINATION" }
    });

    expect(parseFetchBody(0)).toEqual({ bracketMode: "DOUBLE_ELIMINATION" });
});

// Verifies getFlowState returns in-match state.
test("getFlowState returns current state", async () =>
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

    expect(result.state).toBe("IN_MATCH_ACTIVE");
});

// Verifies getBracket returns expected mode.
test("getBracket returns bracket mode", async () =>
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

    expect(result.mode).toBe("DOUBLE_ELIMINATION");
});

// Verifies getCurrentMatch returns selected match id.
test("getCurrentMatch returns match id", async () =>
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

    expect(result.matchId).toBe(currentMatch.matchId);
});

// Verifies submitMatchVote returns committed status.
test("submitMatchVote returns COMMITTED status", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";

    queueTextResponse({
        gameId,
        matchId: "43db2e62-f1fc-4c67-81ca-3578886f3d34",
        status: "COMMITTED",
        voteCount: 2,
        committedWinnerPlayerId: "8aa22920-a8ea-4db7-81d8-91773893ce2a"
    });

    const result = await RequestService<"submitMatchVote", { matchId: string; winnerPlayerId: string }, { status: string }>("submitMatchVote", {
        routeParams: { gameId },
        body: {
            matchId: "43db2e62-f1fc-4c67-81ca-3578886f3d34",
            winnerPlayerId: "8aa22920-a8ea-4db7-81d8-91773893ce2a"
        }
    });

    expect(result.status).toBe("COMMITTED");
});

// Verifies getFlowState route interpolation.
test("getFlowState interpolates route parameter", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";

    queueTextResponse({ state: "IN_MATCH_ACTIVE" });

    await RequestService("getFlowState", {
        routeParams: { gameId }
    });

    expect(getFetchCall(0)[0]).toContain(`/Games/GetFlowState/${gameId}`);
});

// Verifies getBracket route interpolation.
test("getBracket interpolates route parameter", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";

    queueTextResponse({ mode: "DOUBLE_ELIMINATION", matches: [] });

    await RequestService("getBracket", {
        routeParams: { gameId }
    });

    expect(getFetchCall(0)[0]).toContain(`/Games/GetBracket/${gameId}`);
});

// Verifies getCurrentMatch route interpolation.
test("getCurrentMatch interpolates route parameter", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";

    queueTextResponse({ matchId: "match-id", playerOneId: "player-id" });

    await RequestService("getCurrentMatch", {
        routeParams: { gameId }
    });

    expect(getFetchCall(0)[0]).toContain(`/Games/GetCurrentMatch/${gameId}`);
});

// Verifies submitMatchVote route interpolation.
test("submitMatchVote interpolates route parameter", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";

    queueTextResponse({ status: "COMMITTED" });

    await RequestService("submitMatchVote", {
        routeParams: { gameId },
        body: { matchId: "match-id", winnerPlayerId: "winner-id" }
    });

    expect(getFetchCall(0)[0]).toContain(`/Games/SubmitMatchVote/${gameId}`);
});

// Verifies submitMatchVote serializes request body.
test("submitMatchVote serializes winner payload", async () =>
{
    const gameId = "7b6f5f95-0e8e-46c6-bb96-ce52726332d2";
    const payload = {
        matchId: "match-id",
        winnerPlayerId: "winner-id"
    };

    queueTextResponse({ status: "COMMITTED" });

    await RequestService("submitMatchVote", {
        routeParams: { gameId },
        body: payload
    });

    expect(parseFetchBody(0)).toEqual(payload);
});

// Verifies end-to-end lifecycle returns bracket mode.
test("end-to-end lifecycle returns DOUBLE_ELIMINATION mode", async () =>
{
    const gameId = "e9f0525d-bd9f-4d37-8ec0-f1177d53c4e2";
    const playerId = "32187753-4f84-4413-a641-bdd7d101f200";

    queueTextResponse({ GameId: gameId, BracketMode: "DOUBLE_ELIMINATION" });
    queueTextResponse({ message: "Player added" });
    queueTextResponse({ message: "Game started" });
    queueTextResponse({
        gameId,
        mode: "DOUBLE_ELIMINATION",
        gameStarted: true,
        isGrandFinalResetRequired: false,
        players: [
            { playerId, displayName: "Player 1", seed: 1, losses: 0, eliminated: false },
            { playerId: "241f5273-485c-4f40-938e-4ce6bcc1cae6", displayName: "Player 2", seed: 2, losses: 0, eliminated: false }
        ],
        matches: []
    });
    queueTextResponse({
        gameId,
        matchId: "2ecfe5df-f76b-4dca-a762-c0c5326cc9c2",
        lane: "WINNERS",
        round: 1,
        matchNumber: 1,
        playerOneId: playerId,
        playerTwoId: "241f5273-485c-4f40-938e-4ce6bcc1cae6"
    });
    queueTextResponse({
        gameId,
        matchId: "2ecfe5df-f76b-4dca-a762-c0c5326cc9c2",
        status: "COMMITTED",
        voteCount: 2,
        committedWinnerPlayerId: playerId
    });

    await RequestService("createGameWithMode", {
        body: { bracketMode: "DOUBLE_ELIMINATION" }
    });

    await RequestService("addPlayers", {
        routeParams: { gameId },
        body: {
            Id: playerId,
            displayName: "Player 1",
            currentScore: 0,
            currentRound: 0,
            currentCharacter: Marth,
            currentGameId: gameId
        }
    });

    await RequestService("startGame", {
        routeParams: { gameId }
    });

    const snapshot = await RequestService<"getBracket", never, { mode: string }>("getBracket", {
        routeParams: { gameId }
    });

    await RequestService("getCurrentMatch", {
        routeParams: { gameId }
    });

    await RequestService("submitMatchVote", {
        routeParams: { gameId },
        body: {
            matchId: "2ecfe5df-f76b-4dca-a762-c0c5326cc9c2",
            winnerPlayerId: playerId
        }
    });

    expect(snapshot.mode).toBe("DOUBLE_ELIMINATION");
});

// Verifies end-to-end lifecycle calls create route.
test("end-to-end lifecycle calls CreateGameWithMode route", async () =>
{
    const gameId = "e9f0525d-bd9f-4d37-8ec0-f1177d53c4e2";

    queueTextResponse({ GameId: gameId, BracketMode: "DOUBLE_ELIMINATION" });

    await RequestService("createGameWithMode", {
        body: { bracketMode: "DOUBLE_ELIMINATION" }
    });

    expect(getFetchCall(0)[0]).toContain("/Games/CreateGameWithMode");
});

// Verifies vote-ledger request returns pending status.
test("submitMatchVote vote-ledger returns PENDING status", async () =>
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
});

// Verifies vote-ledger request returns expected vote count.
test("submitMatchVote vote-ledger returns vote count", async () =>
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

    expect(result.voteCount).toBe(1);
});

// Verifies vote-ledger request calls expected route.
test("submitMatchVote vote-ledger calls expected route", async () =>
{
    const gameId = "8f29daff-adfa-49bb-a8c8-4e4f9fb58e3b";

    queueTextResponse({
        gameId,
        matchId: "4d4da6f2-c56e-4c34-8d99-1bb3f12ec0dc",
        status: "PENDING",
        voteCount: 1,
        committedWinnerPlayerId: null
    });

    await RequestService("submitMatchVote", {
        routeParams: { gameId },
        body: {
            matchId: "4d4da6f2-c56e-4c34-8d99-1bb3f12ec0dc",
            winnerPlayerId: "fd723edf-9302-42f7-af35-07b2c0604ffc"
        }
    });

    expect(getFetchCall(0)[0]).toContain(`/Games/SubmitMatchVote/${gameId}`);
});

// Verifies vote-ledger request serializes payload.
test("submitMatchVote vote-ledger serializes payload", async () =>
{
    const gameId = "8f29daff-adfa-49bb-a8c8-4e4f9fb58e3b";
    const payload = {
        matchId: "4d4da6f2-c56e-4c34-8d99-1bb3f12ec0dc",
        winnerPlayerId: "fd723edf-9302-42f7-af35-07b2c0604ffc"
    };

    queueTextResponse({
        gameId,
        matchId: payload.matchId,
        status: "PENDING",
        voteCount: 1,
        committedWinnerPlayerId: null
    });

    await RequestService("submitMatchVote", {
        routeParams: { gameId },
        body: payload
    });

    expect(parseFetchBody(0)).toEqual(payload);
});
