import { useEffect, useState } from "react";
import { AnimatePresence, motion, useReducedMotion } from "framer-motion";

// Each word of the heading reveals in sequence. The proposal's intended
// cadence is 0.18 s per word, which lines up with the bracket motion
// config's round stagger so the bracket and the overlay feel related.
const HEADING_WORDS = ["TOURNAMENT", "CHAMPION"];

type WinnerCelebrationProps = {
    // The display name of the player who won the tournament. The component
    // splits the name into individual words for the per-word reveal.
    championName: string;
    // Whether the overlay is visible. Mounting the component with `show` true
    // triggers the entrance animation; flipping to false plays the exit.
    show: boolean;
    // Auto-dismiss timer. Like `CelebrationOverlay`, the parent owns the
    // `show` flag and the overlay fires a custom event when the timer ends.
    autoDismissMs?: number;
    // Optional footer content. The proposal suggests a "view bracket" CTA so
    // the user is not blocked from re-examining the result.
    footer?: React.ReactNode;
};

// Plays a single full-screen celebration when a tournament finishes. The
// overlay is non-blocking by default — the footer area is the only place
// that accepts pointer events — so a player who wants to see the bracket
// again can dismiss it from the same screen.
const WinnerCelebration = ({
    championName,
    show,
    autoDismissMs = 4200,
    footer,
}: WinnerCelebrationProps) =>
{
    const prefersReducedMotion = useReducedMotion();
    const [dismissedAt, setDismissedAt] = useState<number | null>(null);

    useEffect(() =>
    {
        if (!show)
        {
            return;
        }
        setDismissedAt(null);

        const timeoutId = window.setTimeout(() =>
        {
            setDismissedAt(Date.now());
            window.dispatchEvent(new CustomEvent("smash-tourney:winner-done"));
        }, autoDismissMs);

        return () => window.clearTimeout(timeoutId);
    }, [autoDismissMs, show]);

    return (
        <AnimatePresence>
            {show && (
                <motion.div
                    key="winner-celebration"
                    role="status"
                    aria-live="polite"
                    initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0 }}
                    animate={prefersReducedMotion ? { opacity: 1 } : { opacity: 1 }}
                    exit={prefersReducedMotion ? { opacity: 0 } : { opacity: 0 }}
                    transition={{ duration: 0.4 }}
                    className="pointer-events-none absolute inset-0 z-40 flex flex-col items-center justify-center gap-4 text-center bg-gradient-to-b from-yellow-500/10 via-black/40 to-purple-700/20"
                >
                    <div className="pointer-events-none flex flex-col items-center gap-3">
                        <p className="text-yellow-300 text-sm uppercase tracking-[0.4em] font-bold">
                            🏆 🏆 🏆
                        </p>
                        <div className="flex flex-wrap justify-center gap-2">
                            {HEADING_WORDS.map((word, wordIndex) => (
                                <motion.span
                                    key={word}
                                    initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, y: -10 }}
                                    animate={prefersReducedMotion ? { opacity: 1 } : { opacity: 1, y: 0 }}
                                    transition={{ delay: wordIndex * 0.18, duration: 0.3 }}
                                    className="text-2xl md:text-3xl font-extrabold uppercase tracking-widest text-yellow-300"
                                >
                                    {word}
                                </motion.span>
                            ))}
                        </div>
                        <motion.h1
                            initial={prefersReducedMotion
                                ? { opacity: 0 }
                                : { opacity: 0, letterSpacing: "0.6em" }}
                            animate={prefersReducedMotion
                                ? { opacity: 1 }
                                : { opacity: 1, letterSpacing: "0.15em" }}
                            transition={{ delay: 0.5, duration: 0.8, ease: "easeOut" }}
                            className="text-3xl md:text-5xl font-black text-white bracket-shine"
                        >
                            {championName}
                        </motion.h1>
                        <p className="text-yellow-300/90 text-sm uppercase tracking-widest mt-1">
                            Tournament complete
                        </p>
                    </div>
                    {footer && (
                        <div className="pointer-events-auto">
                            {footer}
                        </div>
                    )}
                    {dismissedAt && (
                        <p className="text-white/50 text-xs" aria-hidden="true">tap to dismiss</p>
                    )}
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default WinnerCelebration;
