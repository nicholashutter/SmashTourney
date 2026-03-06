import { expect, test } from "vitest";
import { CurrentMatchResponse } from "../src/models/entities/Bracket";
import { resolveInMatchRedirect } from "../src/services/inMatchRouting";

const activeMatch: CurrentMatchResponse = {
    gameId: "game-id",
    matchId: "match-id",
    lane: "WINNERS",
    round: 1,
    matchNumber: 1,
    playerOneId: "player-1",
    playerTwoId: "player-2"
};

// Verifies no redirect when backend state is unavailable.
test("resolveInMatchRedirect returns null when game state is unavailable", () =>
{
    const redirect = resolveInMatchRedirect(null, activeMatch, "player-1");
    expect(redirect).toBeNull();
});

// Verifies redirect to lobby when game returns to lobby waiting state.
test("resolveInMatchRedirect routes to lobby for LOBBY_WAITING", () =>
{
    const redirect = resolveInMatchRedirect("LOBBY_WAITING", activeMatch, "player-1");
    expect(redirect).toBe("/lobby");
});

// Verifies redirect to bracket when backend reports bracket view.
test("resolveInMatchRedirect routes to showBracket for BRACKET_VIEW", () =>
{
    const redirect = resolveInMatchRedirect("BRACKET_VIEW", activeMatch, "player-1");
    expect(redirect).toBe("/showBracket");
});

// Verifies redirect to bracket when game is complete.
test("resolveInMatchRedirect routes to showBracket for COMPLETE", () =>
{
    const redirect = resolveInMatchRedirect("COMPLETE", activeMatch, "player-1");
    expect(redirect).toBe("/showBracket");
});

// Verifies redirect when active match disappeared during in-match state.
test("resolveInMatchRedirect routes to showBracket when IN_MATCH_ACTIVE has no current match", () =>
{
    const redirect = resolveInMatchRedirect("IN_MATCH_ACTIVE", null, "player-1");
    expect(redirect).toBe("/showBracket");
});

// Verifies non-participants are routed back to bracket when another match is active.
test("resolveInMatchRedirect routes non-participant to showBracket", () =>
{
    const redirect = resolveInMatchRedirect("IN_MATCH_ACTIVE", activeMatch, "player-3");
    expect(redirect).toBe("/showBracket");
});

// Verifies current participants remain on in-match screen.
test("resolveInMatchRedirect keeps participant on in-match", () =>
{
    const redirect = resolveInMatchRedirect("IN_MATCH_ACTIVE", activeMatch, "player-1");
    expect(redirect).toBeNull();
});
