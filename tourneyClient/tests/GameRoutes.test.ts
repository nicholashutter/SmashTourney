import { expect, test } from "vitest";
import { inMatchPath, lobbyPath, routeForGameState, showBracketPath } from "../src/services/gameRoutes";

// Verifies each game screen path carries the game id.
test("game paths embed the game id", () =>
{
    expect(lobbyPath("abc")).toBe("/lobby/abc");
    expect(inMatchPath("abc")).toBe("/inMatch/abc");
    expect(showBracketPath("abc")).toBe("/showBracket/abc");
});

// Verifies a returning player is sent to the screen matching server state.
test("routeForGameState sends a returning player to the screen their game is on", () =>
{
    expect(routeForGameState("LOBBY_WAITING", "abc")).toBe("/lobby/abc");
    expect(routeForGameState("IN_MATCH_ACTIVE", "abc")).toBe("/inMatch/abc");
    expect(routeForGameState("BRACKET_VIEW", "abc")).toBe("/showBracket/abc");
});

// Verifies a finished tournament resolves to the bracket rather than nowhere.
//
// COMPLETE is the state a player is most likely to reopen a link on, since the
// tournament is over and someone is looking up who won.
test("routeForGameState routes a completed game to the bracket", () =>
{
    expect(routeForGameState("COMPLETE", "abc")).toBe("/showBracket/abc");
});
