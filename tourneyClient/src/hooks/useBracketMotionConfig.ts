import { useReducedMotion } from "framer-motion";

// Centralised motion timings for the bracket and surrounding components.
//
// Every bracket animation reads its values from this hook, which means turning
// `prefers-reduced-motion` on in the OS collapses every entry animation, every
// pulse, and every staggered reveal to a single 0.15 s opacity fade. The hook
// is intentionally side-effect free and dependency-free so it can be called
// from `DynamicBracket`, `CelebrationOverlay`, `WinnerCelebration`, and any
// future bracket surface without dragging in extra modules.
export type BracketMotionConfig = {
    prefersReducedMotion: boolean;

    // Delay between rounds when staggering connector / match-box reveals.
    // Picked to feel paced without dragging the whole bracket out by seconds.
    roundStaggerSeconds: number;

    // Path-draw duration for winner-flow connectors. The spring here is the
    // single biggest visible win in the bracket, so it stays long even with
    // reduced motion disabled.
    connectorDrawSeconds: number;

    // Match-box fade-in. Short so the bracket is fully present in <0.5 s after
    // the bracket snapshot first arrives on a phone over a party wifi.
    matchFadeInSeconds: number;

    // Active-match LIVE pulse. Long enough to read as a heartbeat, short
    // enough that a screenshot still looks calm.
    livePulseSeconds: number;

    // Stagger between new-round columns during the round-reveal reveal.
    columnStaggerSeconds: number;

    // Single fade for everything under reduced motion. Keeps the bracket
    // visible rather than teleporting in.
    reducedFadeSeconds: number;
};

// Builds the motion config the bracket and overlays should use this render.
//
// Calling `useReducedMotion()` here means every consumer of the hook shares the
// same OS preference signal, and a future change to the source of that signal
// (e.g. a per-user override) only has to land in this one file.
export const useBracketMotionConfig = (): BracketMotionConfig =>
{
    const prefersReducedMotion = useReducedMotion() ?? false;

    if (prefersReducedMotion)
    {
        return {
            prefersReducedMotion: true,
            roundStaggerSeconds: 0,
            connectorDrawSeconds: 0.01,
            matchFadeInSeconds: 0.15,
            livePulseSeconds: 0,
            columnStaggerSeconds: 0,
            reducedFadeSeconds: 0.15,
        };
    }

    return {
        prefersReducedMotion: false,
        roundStaggerSeconds: 0.35,
        connectorDrawSeconds: 1.2,
        matchFadeInSeconds: 0.35,
        livePulseSeconds: 1.6,
        columnStaggerSeconds: 0.08,
        reducedFadeSeconds: 0.15,
    };
};

export default useBracketMotionConfig;
