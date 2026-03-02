/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */

import { test, expect, vi, beforeEach } from 'vitest';
import { ApplicationUser } from '../src/models/entities/ApplicationUser';
import { RequestService } from '../src/services/RequestService';
import Marth from '../src/models/entities/Characters/Marth';
import { CharacterId } from '../src/models/Enums/CharacterId';
import { Player } from '../src/models/entities/Player';

const UserName = "TestUser";
const Password = "P@SSW0RD123!"

const User: ApplicationUser =
{
    UserName,
    Password
}

beforeEach(
    () => 
    {
        global.fetch = vi.fn();
    }
)

/**
 *  Verifies that RequestService formats a basic POST request correctly.
 * - Endpoint: createUserSession
 * - Payload: ApplicationUser
 * - Checks method, headers, body structure, and response parsing.
 */
test("RequestService submits a properly formatted http request", async () =>
{

    (fetch as any).mockResolvedValueOnce(
        {
            ok: true,
            json: (async: any) => (
                {
                    success: true
                }
            )
        }
    );

    //RequestService is a custom api to wrap fetch that includes type safety for back end
    const result = await RequestService("createUserSession",
        {
            body:
            {
                User
            }
        });
    expect(result).toEqual(
        {
            success: true
        }
    )

    //Using mock provided fetch test that shape of data is correct and reponse returns success
    const [url, options] = (fetch as any).mock.calls[0];

    expect(url).toContain("CreateUserSession");

    expect(options.method).toBe("POST");
    expect(options.headers["Content-Type"]).toBe("application/json");

    const parsedBody = JSON.parse(options.body);
    expect(parsedBody.User).toEqual(User);

})

/**
 *  Verifies that RequestService formats a POST request with route param and array of Player objects.
 * - Endpoint: addPlayers
 * - Payload: Player[]
 * - Route param: gameId
 * - Checks nested Character serialization and response message.
 */
test("RequestService formats POST request with array of Player objects for addPlayers", async () =>
{
    const gameId = "game-abc123";

    const players: Player[] = [
        {
            displayName: "Player One",
            currentScore: 0,
            currentRound: 1,
            currentGameId: gameId,
            currentCharacter: Marth
        },
        {
            displayName: "Player Two",
            currentScore: 0,
            currentRound: 1,
            currentGameId: gameId,
            currentCharacter: Marth
        }
    ];

    (fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({
            message: `Players Added to Game ${gameId}`
        })
    });

    const result = await RequestService("addPlayers", {
        routeParams: { gameId },
        body: players
    });

    expect(result).toEqual({
        message: `Players Added to Game ${gameId}`
    });

    const [url, options] = (fetch as any).mock.calls[0];
    expect(url).toContain(`/Games/AddPlayer/${gameId}`);
    expect(options.method).toBe("POST");
    expect(options.headers["Content-Type"]).toBe("application/json");

    const parsedBody = JSON.parse(options.body);
    expect(Array.isArray(parsedBody)).toBe(true);
    expect(parsedBody[0].displayName).toBe("Player One");
    expect(parsedBody[0].currentCharacter.id).toBe(CharacterId.Marth);
});

test("RequestService addPlayers payload can omit userId", async () =>
{
    const gameId = "f9c2d4eb-8786-449a-ae28-7823813339b6";

    const playerPayload = {
        Id: "93f00506-bcc6-4d0d-a5f0-c588505f3b60",
        displayName: "NoUserIdClientPayload",
        currentScore: 0,
        currentRound: 0,
        currentCharacter: Marth,
        currentGameId: gameId
    };

    (fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({ success: true })
    });

    const result = await RequestService("addPlayers", {
        routeParams: { gameId },
        body: playerPayload
    });

    expect(result).toEqual({ success: true });

    const [url, options] = (fetch as any).mock.calls[0];
    expect(url).toContain(`/Games/AddPlayer/${gameId}`);
    expect(options.method).toBe("POST");

    const parsedBody = JSON.parse(options.body);
    expect(parsedBody.userId).toBeUndefined();
    expect(parsedBody.displayName).toBe("NoUserIdClientPayload");
});

/**
 *  Verifies that RequestService parses a Game object and nested Player[] from response.
 * - Endpoint: getPlayersInGame
 * - Route param: gameId
 * - Checks nested Character deserialization and Game structure.
 */
test("RequestService parses Game object and Player[] from getPlayersInGame response", async () =>
{
    const gameId = "game-xyz789";

    const mockResponse = {
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
    };

    (fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
    });

    const result = await RequestService("getPlayersInGame", {
        routeParams: { gameId },
        body: {}
    });

    expect(result.gameId).toBe(gameId);
    expect(result.gameName).toBe("Test Game");
    expect(Array.isArray(result.players)).toBe(true);

    const receivedCharacter = result.players[0].currentCharacter;
    expect(receivedCharacter.id).toBe(CharacterId.Marth);

    const [url, options] = (fetch as any).mock.calls[0];
    expect(url).toContain(`/Games/GetPlayersInGame/${gameId}`);
    expect(options.method).toBe("POST");
});

test("CreateTourney flow enforces auth and succeeds when authenticated", async () =>
{
    const gameId = "7f3ebf71-704e-4d34-bca9-eb2852e6f922";

    (fetch as any)
        .mockResolvedValueOnce({
            ok: true,
            headers: { get: () => "application/json" },
            text: async () => JSON.stringify({ GameId: gameId })
        })
        .mockResolvedValueOnce({
            ok: false,
            status: 401,
            headers: { get: () => "application/json" },
            text: async () => ""
        });

    const created = await RequestService<"createGame", never, { GameId: string }>("createGame");
    expect(created.GameId).toBe(gameId);

    await expect(
        RequestService<"sessionStatus", never, { IsAuthenticated: boolean }>("sessionStatus")
    ).rejects.toThrow("HTTP 401");

    expect((fetch as any).mock.calls).toHaveLength(2);
    expect((fetch as any).mock.calls[0][0]).toContain("/Games/CreateGame");
    expect((fetch as any).mock.calls[1][0]).toContain("/users/session");
    expect((fetch as any).mock.calls[0][1].credentials).toBe("include");
    expect((fetch as any).mock.calls[1][1].credentials).toBe("include");

    (fetch as any).mockReset();

    const addPlayerPayload = {
        Id: "0de2acbb-b6f0-4f01-ab0a-3f7fb58d3f57",
        displayName: "AuthedUser",
        currentScore: 0,
        currentRound: 0,
        currentCharacter: Marth,
        currentGameId: gameId
    };

    (fetch as any)
        .mockResolvedValueOnce({
            ok: true,
            headers: { get: () => "application/json" },
            text: async () => JSON.stringify({ GameId: gameId })
        })
        .mockResolvedValueOnce({
            ok: true,
            headers: { get: () => "application/json" },
            text: async () => JSON.stringify({ IsAuthenticated: true, UserName: "dummy01" })
        })
        .mockResolvedValueOnce({
            ok: true,
            headers: { get: () => "application/json" },
            text: async () => JSON.stringify({ success: true })
        });

    const createdAuthed = await RequestService<"createGame", never, { GameId: string }>("createGame");
    expect(createdAuthed.GameId).toBe(gameId);

    const session = await RequestService<"sessionStatus", never, { IsAuthenticated: boolean; UserName: string }>("sessionStatus");
    expect(session.IsAuthenticated).toBe(true);
    expect(session.UserName).toBe("dummy01");

    const addResult = await RequestService("addPlayers", {
        routeParams: { gameId },
        body: addPlayerPayload
    });

    expect(addResult).toEqual({ success: true });
    expect((fetch as any).mock.calls).toHaveLength(3);
    expect((fetch as any).mock.calls[2][0]).toContain(`/Games/AddPlayer/${gameId}`);
    expect((fetch as any).mock.calls[2][1].credentials).toBe("include");
});

test("RequestService createGameWithMode sends bracketMode payload", async () =>
{
    (fetch as any).mockResolvedValueOnce({
        ok: true,
        headers: { get: () => "application/json" },
        text: async () => JSON.stringify({ gameId: "mode-game-id", bracketMode: "DOUBLE_ELIMINATION" })
    });

    const result = await RequestService<"createGameWithMode", { bracketMode: string }, { gameId: string; bracketMode: string }>(
        "createGameWithMode",
        {
            body: { bracketMode: "DOUBLE_ELIMINATION" }
        }
    );

    expect(result.gameId).toBe("mode-game-id");
    expect(result.bracketMode).toBe("DOUBLE_ELIMINATION");

    const [url, options] = (fetch as any).mock.calls[0];
    expect(url).toContain("/Games/CreateGameWithMode");
    expect(options.method).toBe("POST");
    expect(JSON.parse(options.body)).toEqual({ bracketMode: "DOUBLE_ELIMINATION" });
});

test("RequestService bracket routes interpolate gameId and parse responses", async () =>
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

    const bracketSnapshot = {
        gameId,
        mode: "DOUBLE_ELIMINATION",
        gameStarted: true,
        isGrandFinalResetRequired: false,
        players: [],
        matches: []
    };

    (fetch as any)
        .mockResolvedValueOnce({
            ok: true,
            headers: { get: () => "application/json" },
            text: async () => JSON.stringify(bracketSnapshot)
        })
        .mockResolvedValueOnce({
            ok: true,
            headers: { get: () => "application/json" },
            text: async () => JSON.stringify(currentMatch)
        })
        .mockResolvedValueOnce({
            ok: true,
            status: 204,
            headers: { get: () => "" },
            text: async () => ""
        });

    const snapshotResult = await RequestService<"getBracket", never, typeof bracketSnapshot>("getBracket", {
        routeParams: { gameId }
    });

    const currentMatchResult = await RequestService<"getCurrentMatch", never, typeof currentMatch>("getCurrentMatch", {
        routeParams: { gameId }
    });

    const reportResult = await RequestService<"reportMatch", { matchId: string; winnerPlayerId: string }, void>("reportMatch", {
        routeParams: { gameId },
        body: {
            matchId: currentMatch.matchId,
            winnerPlayerId: currentMatch.playerOneId
        }
    });

    expect(snapshotResult.mode).toBe("DOUBLE_ELIMINATION");
    expect(currentMatchResult.matchId).toBe(currentMatch.matchId);
    expect(reportResult).toBeUndefined();

    expect((fetch as any).mock.calls[0][0]).toContain(`/Games/GetBracket/${gameId}`);
    expect((fetch as any).mock.calls[1][0]).toContain(`/Games/GetCurrentMatch/${gameId}`);
    expect((fetch as any).mock.calls[2][0]).toContain(`/Games/ReportMatch/${gameId}`);
    expect(JSON.parse((fetch as any).mock.calls[2][1].body)).toEqual({
        matchId: currentMatch.matchId,
        winnerPlayerId: currentMatch.playerOneId
    });
});
