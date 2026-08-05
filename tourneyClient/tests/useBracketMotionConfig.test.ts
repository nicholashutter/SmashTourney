// @vitest-environment jsdom

import { afterEach, beforeEach, expect, test, vi } from "vitest";
import { renderHook } from "@testing-library/react";

// framer-motion's `useReducedMotion` is mocked per-test so the hook can be
// exercised in both modes without driving the OS-level media query.
const mockUseReducedMotion = vi.fn();

vi.mock("framer-motion", async () =>
{
    const actual = await vi.importActual<typeof import("framer-motion")>("framer-motion");
    return {
        ...actual,
        useReducedMotion: () => mockUseReducedMotion(),
    };
});

beforeEach(() =>
{
    mockUseReducedMotion.mockReset();
});

afterEach(() =>
{
    vi.restoreAllMocks();
});

// Verifies the hook returns the full-motion timings when the user has not
// requested reduced motion.
test("returns the full-motion timings when prefers-reduced-motion is off", async () =>
{
    mockUseReducedMotion.mockReturnValue(false);
    const { useBracketMotionConfig } = await import("../src/hooks/useBracketMotionConfig");
    const { result } = renderHook(() => useBracketMotionConfig());

    expect(result.current.prefersReducedMotion).toBe(false);
    expect(result.current.roundStaggerSeconds).toBeGreaterThan(0);
    expect(result.current.connectorDrawSeconds).toBeGreaterThan(0);
    expect(result.current.matchFadeInSeconds).toBeGreaterThan(0);
    expect(result.current.livePulseSeconds).toBeGreaterThan(0);
    expect(result.current.columnStaggerSeconds).toBeGreaterThan(0);
});

// Verifies the hook returns the reduced-motion timings when the user has
// requested reduced motion. Round staggers collapse to 0 so the bracket does
// not delay its appearance for the user.
test("returns the reduced-motion timings when prefers-reduced-motion is on", async () =>
{
    mockUseReducedMotion.mockReturnValue(true);
    const { useBracketMotionConfig } = await import("../src/hooks/useBracketMotionConfig");
    const { result } = renderHook(() => useBracketMotionConfig());

    expect(result.current.prefersReducedMotion).toBe(true);
    expect(result.current.roundStaggerSeconds).toBe(0);
    expect(result.current.connectorDrawSeconds).toBeLessThan(0.1);
    expect(result.current.livePulseSeconds).toBe(0);
    expect(result.current.columnStaggerSeconds).toBe(0);
    expect(result.current.reducedFadeSeconds).toBeGreaterThan(0);
});
