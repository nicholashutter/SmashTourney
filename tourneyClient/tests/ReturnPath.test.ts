import { expect, test } from "vitest";
import { readReturnPath, signInPathReturningTo } from "../src/services/returnPath";

// Verifies a game URL survives a round trip through the sign-in redirect.
test("a game path survives being carried through sign-in", () =>
{
    const signInPath = signInPathReturningTo("/showBracket/game-abc");
    const search = signInPath.slice(signInPath.indexOf("?"));

    expect(readReturnPath(search)).toBe("/showBracket/game-abc");
});

// Verifies an ordinary visit to the sign-in page has no destination.
test("readReturnPath returns null when no destination was carried", () =>
{
    expect(readReturnPath("")).toBeNull();
    expect(readReturnPath("?other=1")).toBeNull();
});

// Verifies a destination pointing off-site is refused.
//
// A protocol-relative value would send a freshly signed-in player to another
// origin, which is the shape an open redirect takes.
test("readReturnPath refuses destinations that leave the site", () =>
{
    expect(readReturnPath("?next=https://evil.example.com")).toBeNull();
    expect(readReturnPath("?next=//evil.example.com")).toBeNull();
    expect(readReturnPath("?next=evil")).toBeNull();
});

// Verifies a query string on the destination is preserved.
test("readReturnPath keeps a destination's own query string", () =>
{
    const signInPath = signInPathReturningTo("/joinTourney?gameId=abc");
    const search = signInPath.slice(signInPath.indexOf("?"));

    expect(readReturnPath(search)).toBe("/joinTourney?gameId=abc");
});
