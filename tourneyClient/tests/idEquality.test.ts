import { expect, test } from "vitest";
import { areSameId } from "../src/lib/idEquality";

// Verifies exact identifier strings are treated as equal.
test("areSameId returns true for identical identifiers", () =>
{
    const result = areSameId("abc", "abc");

    expect(result).toBe(true);
});

// Verifies case differences are treated as equal.
test("areSameId returns true for case-insensitive matches", () =>
{
    const result = areSameId("ABC-123", "abc-123");

    expect(result).toBe(true);
});

// Verifies surrounding whitespace is ignored for identifier comparisons.
test("areSameId returns true for trimmed matches", () =>
{
    const result = areSameId("  id-value ", "id-value");

    expect(result).toBe(true);
});

// Verifies missing identifiers are never treated as equal.
test("areSameId returns false when either identifier is missing", () =>
{
    const result = areSameId("id-value", null);

    expect(result).toBe(false);
});
