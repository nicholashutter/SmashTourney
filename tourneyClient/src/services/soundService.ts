import smashSoundUrl from "@/assets/sound/smash.mp3";

// Names of the named events the rest of the app can fire.
//
// The asset is one clip today, so every event resolves to the same source — but
// routing everything through this enum means a future "bonk for vote error"
// only has to add a new case here, with no scattered string keys to chase.
export type SoundEvent = "voteCommit" | "tournamentComplete";

const MUTED_STORAGE_KEY = "smashTourney.muted";

type PlayOptions = {
    // When omitted the clip plays from the start. A positive value seeks into
    // the asset, useful for "play the punch" without the build-up on commits.
    startOffsetSeconds?: number;
};

type SoundStorageState = {
    muted: boolean;
};

// Decodes a localStorage entry as a boolean, treating anything missing or
// unparseable as "not muted". The preference has to survive page reloads
// because the mute toggle is the one place the user actually controls audio.
const readStoredMuted = (): boolean =>
{
    if (typeof window === "undefined")
    {
        return false;
    }

    try
    {
        const raw = window.localStorage.getItem(MUTED_STORAGE_KEY);
        if (raw == null)
        {
            return false;
        }

        return raw === "true";
    }
    catch
    {
        // localStorage can throw in privacy modes; failing open is the safer
        // direction — the user opted into sound if we cannot read state.
        return false;
    }
};

const writeStoredMuted = (muted: boolean): void =>
{
    if (typeof window === "undefined")
    {
        return;
    }

    try
    {
        window.localStorage.setItem(MUTED_STORAGE_KEY, muted ? "true" : "false");
    }
    catch
    {
        // localStorage is best-effort. The in-memory toggle still works for
        // the rest of the session.
    }
};

const subscribers = new Set<(state: SoundStorageState) => void>();

const broadcast = (): void =>
{
    const state: SoundStorageState = { muted: readStoredMuted() };
    subscribers.forEach((subscriber) => subscriber(state));
};

// Returns the current persisted mute state. The first call primes the
// listener-set so subsequent updates can notify subscribers.
export const getSoundState = (): SoundStorageState =>
{
    return { muted: readStoredMuted() };
};

// Flips the mute flag and notifies subscribers. Returns the new value so
// callers (e.g. a toggle button) can reflect the change in their own state.
export const setMuted = (nextMuted: boolean): boolean =>
{
    writeStoredMuted(nextMuted);
    broadcast();
    return nextMuted;
};

// Toggles the current mute state. Convenience for the icon button — same as
// `setMuted(!getSoundState().muted)`.
export const toggleMuted = (): boolean =>
{
    const next = !readStoredMuted();
    return setMuted(next);
};

// Subscribes to mute-state changes. The same callback fires for both
// localStorage writes (cross-tab) and explicit `setMuted`/`toggleMuted` calls.
// Returns an unsubscribe function.
export const subscribeSoundState = (
    subscriber: (state: SoundStorageState) => void
): (() => void) =>
{
    subscribers.add(subscriber);
    subscriber({ muted: readStoredMuted() });

    const storageListener = (event: StorageEvent) =>
    {
        if (event.key === MUTED_STORAGE_KEY)
        {
            broadcast();
        }
    };

    if (typeof window !== "undefined")
    {
        window.addEventListener("storage", storageListener);
    }

    return () =>
    {
        subscribers.delete(subscriber);
        if (typeof window !== "undefined")
        {
            window.removeEventListener("storage", storageListener);
        }
    };
};

// Plays a named sound effect when audio is enabled.
//
// The browser's autoplay policy only lets `Audio.play()` succeed after a user
// gesture, so callers should fire this from event handlers (button clicks,
// input changes) — not from `useEffect` on mount. Every other failure mode
// (missing asset, decode error) is swallowed silently because the bracket is
// the foreground, and a missing click is not worth interrupting it for.
//
// The `event` parameter is reserved for per-event clip selection once the
// project has more than one asset. Today every event resolves to the same
// `smash.mp3` and the parameter is read for its presence to satisfy the
// public API.
export const playSound = (event: SoundEvent, options: PlayOptions = {}): void =>
{
    void event;
    if (readStoredMuted())
    {
        return;
    }

    if (typeof window === "undefined")
    {
        return;
    }

    try
    {
        const audio = new Audio(smashSoundUrl);

        if (options.startOffsetSeconds && options.startOffsetSeconds > 0)
        {
            audio.currentTime = options.startOffsetSeconds;
        }

        const playPromise = audio.play();
        if (playPromise && typeof playPromise.catch === "function")
        {
            playPromise.catch(() =>
            {
                // Autoplay rejection or interrupted load — no UI feedback,
                // the click that fired this is more important than the sound.
            });
        }
    }
    catch
    {
        // Audio construction can throw in sandboxed contexts. Silence.
    }
};
