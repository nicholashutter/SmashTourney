import { beforeEach, expect, test, vi } from "vitest";
import { GameSummary } from "../src/models/entities/Bracket";
import
{
    describeGameState,
    endTournament,
    fetchActiveGames,
    gameBrowserDestination,
    resolveGameAction,
    sortGamesForBrowsing
} from "../src/services/gameBrowserService";

const fetchSpy = () =>
{
    return fetch as unknown as ReturnType<typeof vi.fn>;
};

const queueResponse = (payload: unknown, ok = true, status = 200) =>
{
    fetchSpy().mockResolvedValueOnce({
        ok,
        status,
        headers: {
            get: () => "application/json"
        },
        text: async () => JSON.stringify(payload)
    });
};

const buildGame = (overrides: Partial<GameSummary> = {}): GameSummary =>
{
    return {
        gameId: "game-1",
        bracketMode: "SINGLE_ELIMINATION",
        playerCount: 2,
        state: "LOBBY_WAITING",
        createdUtc: "2026-07-29T12:00:00Z",
        isHost: false,
        hasJoined: false,
        ...overrides
    };
};

beforeEach(() =>
{
    global.fetch = vi.fn();
});

// Verifies the browser reads the whole list the server offered.
test("fetchActiveGames returns every tournament the server listed", async () =>
{
    queueResponse([
        buildGame({ gameId: "game-1" }),
        buildGame({ gameId: "game-2", state: "IN_MATCH_ACTIVE", playerCount: 8 })
    ]);

    const games = await fetchActiveGames();

    expect(games).toHaveLength(2);
    expect(games[1].gameId).toBe("game-2");
    expect(games[1].playerCount).toBe(8);
});

// Verifies the list request rides on the identity cookie like every other call.
test("fetchActiveGames asks the games route with credentials attached", async () =>
{
    queueResponse([]);

    await fetchActiveGames();

    const [url, init] = fetchSpy().mock.calls[0] as [string, RequestInit];
    expect(url).toContain("/Games/GetActiveGames");
    expect(init.credentials).toBe("include");
});

// Verifies a malformed payload does not become an exception mid-render.
test("fetchActiveGames yields an empty list when the payload is not a list", async () =>
{
    queueResponse({ message: "unexpected" });

    const games = await fetchActiveGames();

    expect(games).toEqual([]);
});

// Verifies a player already in a game is offered their way back, not a re-join.
test("resolveGameAction offers a rejoin for a game the player is already in", () =>
{
    const action = resolveGameAction(buildGame({ hasJoined: true, state: "IN_MATCH_ACTIVE" }));

    expect(action).toBe("REJOIN");
});

// Verifies a waiting lobby is offered as joinable.
test("resolveGameAction offers a join for a lobby the player is not in", () =>
{
    expect(resolveGameAction(buildGame())).toBe("JOIN");
});

// Verifies a started tournament is not offered to an outsider.
//
// AddPlayer refuses a started game server-side, so offering it here would walk
// somebody through character selection only to fail at the last step.
test("resolveGameAction closes a started game to someone who is not in it", () =>
{
    expect(resolveGameAction(buildGame({ state: "IN_MATCH_ACTIVE" }))).toBe("CLOSED");
    expect(resolveGameAction(buildGame({ state: "COMPLETE" }))).toBe("CLOSED");
});

// Verifies a returning player lands on the screen their game is actually on.
test("gameBrowserDestination sends a returning player to their game's screen", () =>
{
    expect(gameBrowserDestination(buildGame({ hasJoined: true, state: "IN_MATCH_ACTIVE" })))
        .toBe("/inMatch/game-1");
    expect(gameBrowserDestination(buildGame({ hasJoined: true, state: "LOBBY_WAITING" })))
        .toBe("/lobby/game-1");
});

// Verifies picking a lobby carries the game id so nobody types a GUID.
test("gameBrowserDestination prefills the join screen with the chosen game id", () =>
{
    expect(gameBrowserDestination(buildGame({ gameId: "abc-123" })))
        .toBe("/joinTourney?gameId=abc-123");
});

// Verifies a closed game offers nowhere to go rather than a broken link.
test("gameBrowserDestination has no destination for a closed game", () =>
{
    expect(gameBrowserDestination(buildGame({ state: "COMPLETE" }))).toBeNull();
});

// Verifies the list puts what a player can act on above what they cannot.
test("sortGamesForBrowsing puts the player's own games first and closed ones last", () =>
{
    const games = [
        buildGame({ gameId: "closed", state: "COMPLETE", createdUtc: "2026-07-29T14:00:00Z" }),
        buildGame({ gameId: "joinable", createdUtc: "2026-07-29T13:00:00Z" }),
        buildGame({ gameId: "mine", hasJoined: true, createdUtc: "2026-07-29T10:00:00Z" })
    ];

    const ordered = sortGamesForBrowsing(games).map((game) => game.gameId);

    expect(ordered).toEqual(["mine", "joinable", "closed"]);
});

// Verifies the newest tournament wins ties, since it is the one being discussed.
test("sortGamesForBrowsing orders equally actionable games newest first", () =>
{
    const games = [
        buildGame({ gameId: "older", createdUtc: "2026-07-29T09:00:00Z" }),
        buildGame({ gameId: "newer", createdUtc: "2026-07-29T15:00:00Z" })
    ];

    const ordered = sortGamesForBrowsing(games).map((game) => game.gameId);

    expect(ordered).toEqual(["newer", "older"]);
});

// Verifies sorting does not rearrange the caller's own array.
test("sortGamesForBrowsing leaves the source list untouched", () =>
{
    const games = [
        buildGame({ gameId: "closed", state: "COMPLETE" }),
        buildGame({ gameId: "mine", hasJoined: true })
    ];

    sortGamesForBrowsing(games);

    expect(games.map((game) => game.gameId)).toEqual(["closed", "mine"]);
});

// Verifies each state is described in words a player reads off a phone.
test("describeGameState says what a tournament is doing in plain words", () =>
{
    expect(describeGameState(buildGame())).toBe("Waiting in lobby");
    expect(describeGameState(buildGame({ state: "IN_MATCH_ACTIVE" }))).toBe("In progress");
    expect(describeGameState(buildGame({ state: "BRACKET_VIEW" }))).toBe("In progress");
    expect(describeGameState(buildGame({ state: "COMPLETE" }))).toBe("Finished");
});

// Verifies the host's end request goes to the right game.
test("endTournament posts to the end route for the chosen game", async () =>
{
    queueResponse("Game ended");

    const outcome = await endTournament("game-9");

    expect(outcome.kind).toBe("ended");
    const [url, init] = fetchSpy().mock.calls[0] as [string, RequestInit];
    expect(url).toContain("/Games/EndGame/game-9");
    expect(init.method).toBe("POST");
    expect(init.credentials).toBe("include");
});

// Verifies a refusal reads as a refusal rather than as a server fault.
//
// This is the case that matters most: a non-host tapping end must be told no,
// and must not be left thinking the request merely failed and is worth retrying.
test("endTournament reports a refusal distinctly from a failure", async () =>
{
    queueResponse({}, false, 403);

    const outcome = await endTournament("game-9");

    expect(outcome.kind).toBe("notHost");
});

// Verifies a game that is already gone is not reported as an error.
test("endTournament reports an already-deleted game as not found", async () =>
{
    queueResponse({}, false, 404);

    const outcome = await endTournament("game-9");

    expect(outcome.kind).toBe("notFound");
});

// Verifies a genuine server fault stays distinguishable from a refusal.
test("endTournament reports a server fault as failed", async () =>
{
    queueResponse({}, false, 500);

    const outcome = await endTournament("game-9");

    expect(outcome.kind).toBe("failed");
});

// Verifies a dropped connection does not read as "that is not your game".
test("endTournament reports a network error as failed", async () =>
{
    fetchSpy().mockRejectedValueOnce(new Error("Failed to fetch"));

    const outcome = await endTournament("game-9");

    expect(outcome.kind).toBe("failed");
});
