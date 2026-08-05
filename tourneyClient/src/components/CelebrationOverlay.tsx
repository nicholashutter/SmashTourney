import { useEffect, useState } from "react";
import { AnimatePresence, motion, useReducedMotion } from "framer-motion";

// Confetti particles are simple SVG circles that fan out from the center.
// Each particle has its own trajectory via CSS custom properties, set inline
// so the keyframe in `index.css` can read them.
const CONFETTI_COLORS = ["#facc15", "#22d3ee", "#a78bfa", "#f472b6", "#34d399", "#fb923c"];

const CONFETTI_COUNT = 8;

const buildConfetti = () =>
{
    return Array.from({ length: CONFETTI_COUNT }, (_, index) =>
    {
        const angle = (Math.PI * 2 * index) / CONFETTI_COUNT;
        const distance = 90 + ((index * 17) % 30);
        return {
            id: index,
            x: Math.cos(angle) * distance,
            y: Math.sin(angle) * distance,
            rotate: (index * 47) % 360,
            color: CONFETTI_COLORS[index % CONFETTI_COLORS.length],
            delaySeconds: (index % 4) * 0.05,
        };
    });
};

type CelebrationOverlayProps = {
    // The display name of the match winner. The overlay title shows this name
    // in the largest text on the panel.
    winnerName: string;
    // Controls whether the overlay is visible. Mounting the component with
    // `show` true triggers the entrance animation; flipping to false plays
    // the exit animation and then unmounts.
    show: boolean;
    // Auto-dismiss after this many milliseconds. The component does not
    // unmount itself; the parent should track the timeout and set `show` to
    // false when it fires. The default of 1400 ms matches the proposal.
    autoDismissMs?: number;
    // Optional content for the lower band. The proposal recommends a "view
    // bracket" CTA so the user is not blocked from interacting with the
    // bracket behind the overlay.
    footer?: React.ReactNode;
};

// Plays a short, non-blocking celebration on top of the in-match panel when a
// vote commits. The overlay is `pointer-events: none` except for the footer
// CTA, so a player who wants to keep reading the screen can tap through.
const CelebrationOverlay = ({
    winnerName,
    show,
    autoDismissMs = 1400,
    footer,
}: CelebrationOverlayProps) =>
{
    const prefersReducedMotion = useReducedMotion();
    const [confetti] = useState(() => buildConfetti());

    useEffect(() =>
    {
        if (!show)
        {
            return;
        }

        const timeoutId = window.setTimeout(() =>
        {
            // The parent owns the show flag. The overlay only signals via a
            // custom event so the in-match page can decide when to unmount.
            window.dispatchEvent(new CustomEvent("smash-tourney:celebration-done"));
        }, autoDismissMs);

        return () => window.clearTimeout(timeoutId);
    }, [autoDismissMs, show]);

    return (
        <AnimatePresence>
            {show && (
                <motion.div
                    key="celebration"
                    role="status"
                    aria-live="polite"
                    initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, scale: 0.85 }}
                    animate={prefersReducedMotion ? { opacity: 1 } : { opacity: 1, scale: 1 }}
                    exit={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, scale: 0.85 }}
                    transition={{ type: "spring", stiffness: 220, damping: 18 }}
                    className="pointer-events-none absolute inset-0 z-30 flex flex-col items-center justify-center gap-3 text-center"
                >
                    <div className="pointer-events-none relative">
                        {/* Confetti ring. The CSS keyframe uses inline custom
                            properties (--confetti-x, --confetti-y) so each
                            particle flies to its own direction. */}
                        {!prefersReducedMotion && (
                            <div className="absolute inset-0 flex items-center justify-center" aria-hidden="true">
                                {confetti.map((particle) => (
                                    <span
                                        key={particle.id}
                                        className="bracket-confetti-particle"
                                        style={{
                                            ["--confetti-x" as never]: `${particle.x}px`,
                                            ["--confetti-y" as never]: `${particle.y}px`,
                                            ["--confetti-rotate" as never]: `${particle.rotate}deg`,
                                            animationDelay: `${particle.delaySeconds}s`,
                                            backgroundColor: particle.color,
                                            width: "10px",
                                            height: "10px",
                                            borderRadius: "2px",
                                            position: "absolute",
                                        }}
                                    />
                                ))}
                            </div>
                        )}

                        <div className="bg-black/70 rounded-2xl px-6 py-5 shadow-2xl ring-2 ring-yellow-300/60">
                            <p className="text-yellow-300 text-sm uppercase tracking-widest font-bold">
                                Match result
                            </p>
                            <p className="text-3xl font-extrabold text-white mt-1 break-words">
                                {winnerName}
                            </p>
                            <p className="text-yellow-300/90 text-base mt-2">wins the match</p>
                        </div>
                    </div>
                    {footer && (
                        <div className="pointer-events-auto">
                            {footer}
                        </div>
                    )}
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default CelebrationOverlay;
