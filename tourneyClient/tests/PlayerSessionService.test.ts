import { beforeEach, expect, test, vi } from "vitest";
import { resumePlayerSession } from "../src/services/playerSessionService";

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

beforeEach(() =>
{
    global.fetch = vi.fn();
});

// Verifies a dropped player's session is rebuilt from the game id alone.
test("resumePlayerSession returns the session the server resolved", async () =>
{
    queueResponse({
        gameId: "game-abc",
        playerId: "player-1",
        displayName: "Nick",
        isHost: true,
        state: "IN_MATCH_ACTIVE",
        gameStarted: true
    });

    const outcome = await resumePlayerSession("game-abc");

    expect(outcome.kind).toBe("resumed");
    if (outcome.kind === "resumed")
    {
        expect(outcome.session.playerId).toBe("player-1");
        expect(outcome.session.isHost).toBe(true);
        expect(outcome.session.state).toBe("IN_MATCH_ACTIVE");
    }
});

// Verifies the request carries only the game id and rides on the auth cookie.
//
// This is the whole reason a reopened URL can work: the id comes from the path
// and the identity comes from a cookie the tab loss did not take with it.
test("resumePlayerSession asks by game id with credentials attached", async () =>
{
    queueResponse({
        gameId: "game-abc",
        playerId: "player-1",
        displayName: "Nick",
        isHost: false,
        state: "LOBBY_WAITING",
        gameStarted: false
    });

    await resumePlayerSession("game-abc");

    const [url, init] = fetchSpy().mock.calls[0] as [string, RequestInit];
    expect(url).toContain("/Games/GetPlayerSession/game-abc");
    expect(init.credentials).toBe("include");
});

// Verifies a non-participant is reported as such rather than as a failure.
//
// Opening a link to a game you have not joined is an ordinary thing to do, and
// the client turns this outcome into a trip to the join screen.
test("resumePlayerSession reports a non-participant distinctly from an error", async () =>
{
    queueResponse({}, false, 404);

    const outcome = await resumePlayerSession("game-abc");

    expect(outcome.kind).toBe("notParticipant");
});

// Verifies a genuine failure is not mistaken for a non-participant.
test("resumePlayerSession reports a server failure as failed", async () =>
{
    queueResponse({}, false, 500);

    const outcome = await resumePlayerSession("game-abc");

    expect(outcome.kind).toBe("failed");
});

// Verifies a dropped connection does not read as "you are not in this game".
test("resumePlayerSession reports a network error as failed", async () =>
{
    fetchSpy().mockRejectedValueOnce(new Error("Failed to fetch"));

    const outcome = await resumePlayerSession("game-abc");

    expect(outcome.kind).toBe("failed");
});
