// @vitest-environment jsdom

import { afterEach, beforeEach, expect, test, vi } from "vitest";

const STORAGE_KEY = "smashTourney.muted";

// Audio construction is stubbed via a real class so the `new` call records
// a real instance. A class beats `vi.fn()` here because the sound service
// uses `new Audio(...)` as a constructor and vitest's `vi.fn()` does not
// always behave as a constructor across versions.
class AudioMock
{
    static instances: AudioMock[] = [];
    play = vi.fn().mockResolvedValue(undefined);
    currentTime = 0;
    constructor()
    {
        AudioMock.instances.push(this);
    }
}

vi.stubGlobal("Audio", AudioMock);

// Polyfill a minimal `localStorage` on the jsdom `window` because the
// sound service guards every read against a missing `window` — the guard
// returns `false` instead of throwing, but the tests need a real store
// they can read back. A `Map`-backed shim is enough for the assertions
// below.
const storage = new Map<string, string>();
const storageShim = {
    clear: () => storage.clear(),
    getItem: (key: string) => storage.has(key) ? storage.get(key)! : null,
    setItem: (key: string, value: string) => { storage.set(key, String(value)); },
    removeItem: (key: string) => { storage.delete(key); },
    key: (index: number) => Array.from(storage.keys())[index] ?? null,
    get length() { return storage.size; },
};

Object.defineProperty(window, "localStorage", {
    configurable: true,
    value: storageShim,
});

beforeEach(() =>
{
    storage.clear();
    AudioMock.instances.length = 0;
});

afterEach(() =>
{
    vi.restoreAllMocks();
});

// Verifies playSound respects the muted flag.
test("playSound is a no-op when muted is true", async () =>
{
    window.localStorage.setItem(STORAGE_KEY, "true");
    const { playSound } = await import("../src/services/soundService");

    playSound("voteCommit");

    expect(AudioMock.instances.length).toBe(0);
});

// Verifies playSound creates an Audio element when unmuted.
test("playSound constructs an Audio element when unmuted", async () =>
{
    window.localStorage.removeItem(STORAGE_KEY);
    const { playSound } = await import("../src/services/soundService");

    playSound("voteCommit");

    expect(AudioMock.instances.length).toBe(1);
    expect(AudioMock.instances[0].play).toHaveBeenCalledTimes(1);
});

// Verifies playSound applies the startOffsetSeconds option as currentTime.
test("playSound applies startOffsetSeconds to the audio currentTime", async () =>
{
    window.localStorage.removeItem(STORAGE_KEY);
    const { playSound } = await import("../src/services/soundService");

    playSound("tournamentComplete", { startOffsetSeconds: 1.5 });

    expect(AudioMock.instances[0].currentTime).toBe(1.5);
});

// Verifies toggleMuted flips the muted flag and notifies subscribers.
test("toggleMuted flips the muted flag", async () =>
{
    const { getSoundState, subscribeSoundState, toggleMuted } = await import("../src/services/soundService");
    const subscriber = vi.fn();
    const unsubscribe = subscribeSoundState(subscriber);

    expect(getSoundState().muted).toBe(false);

    toggleMuted();

    expect(getSoundState().muted).toBe(true);
    expect(window.localStorage.getItem(STORAGE_KEY)).toBe("true");
    expect(subscriber).toHaveBeenCalled();

    unsubscribe();
});

// Verifies setMuted writes the storage key for cross-tab sync.
test("setMuted persists the value to localStorage", async () =>
{
    const { setMuted } = await import("../src/services/soundService");

    setMuted(true);
    expect(window.localStorage.getItem(STORAGE_KEY)).toBe("true");

    setMuted(false);
    expect(window.localStorage.getItem(STORAGE_KEY)).toBe("false");
});
