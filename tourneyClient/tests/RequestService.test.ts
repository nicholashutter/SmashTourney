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
    expect(url).toContain(`/Games/AddPlayers/${gameId}`);
    expect(options.method).toBe("POST");
    expect(options.headers["Content-Type"]).toBe("application/json");

    const parsedBody = JSON.parse(options.body);
    expect(Array.isArray(parsedBody)).toBe(true);
    expect(parsedBody[0].displayName).toBe("Player One");
    expect(parsedBody[0].currentCharacter.id).toBe(CharacterId.Marth);
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
