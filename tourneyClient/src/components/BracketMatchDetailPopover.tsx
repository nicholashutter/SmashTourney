import { useEffect, useMemo, useRef } from "react";
import { motion, useReducedMotion } from "framer-motion";
import { BracketMatchView, BracketPlayerView } from "@/models/entities/Bracket";

type Props = {
    // The match the popover is describing. Null collapses the popover.
    match: BracketMatchView | null;
    // The player lookup so the popover can resolve display names and seed
    // information without re-deriving them from the bracket.
    playersById: Map<string, BracketPlayerView>;
    // The bracket container's bounding rect. The popover reads the
    // container's box to map SVG viewBox coordinates back to pixels.
    containerRect: DOMRect | null;
    // The position of the match in the bracket's viewBox coordinates.
    matchPosition: { x: number; y: number } | null;
    // The viewBox of the bracket, used to map SVG units to pixels.
    viewBox: { width: number; height: number } | null;
    // Closes the popover. Used by the Escape handler and the outside click
    // detector so a11y users can dismiss without re-focusing the match.
    onClose: () => void;
};

// Renders a small panel near a match with the seed, losses, and elimination
// state of both participants. The proposal calls for a "W/L last 3" form
// indicator; the API does not return that yet, so the popover omits it. The
// only data the popover renders is what `BracketPlayerView` already carries.
const BracketMatchDetailPopover = ({
    match,
    playersById,
    containerRect,
    matchPosition,
    viewBox,
    onClose,
}: Props) =>
{
    const prefersReducedMotion = useReducedMotion();
    const popoverRef = useRef<HTMLDivElement | null>(null);

    // Resolves the on-screen coordinates for the popover anchor. The match's
    // viewBox position is converted to a pixel offset inside the container.
    const pixelPosition = useMemo(() =>
    {
        if (!matchPosition || !containerRect || !viewBox)
        {
            return null;
        }

        const scaleX = containerRect.width / viewBox.width;
        const scaleY = containerRect.height / viewBox.height;
        return {
            left: matchPosition.x * scaleX,
            top: matchPosition.y * scaleY,
        };
    }, [containerRect, matchPosition, viewBox]);

    // Closes the popover on Escape. The match group that opened it also
    // responds to Escape via the same effect; both listeners coexist so the
    // close is responsive regardless of which element has focus.
    useEffect(() =>
    {
        if (!match)
        {
            return;
        }

        const handleKey = (event: KeyboardEvent) =>
        {
            if (event.key === "Escape")
            {
                onClose();
            }
        };
        window.addEventListener("keydown", handleKey);
        return () => window.removeEventListener("keydown", handleKey);
    }, [match, onClose]);

    if (!match || !pixelPosition)
    {
        return null;
    }

    const one = match.playerOneId ? playersById.get(match.playerOneId) : undefined;
    const two = match.playerTwoId ? playersById.get(match.playerTwoId) : undefined;

    const renderPlayer = (player: BracketPlayerView | undefined, isWinner: boolean) =>
    {
        if (!player)
        {
            return (
                <div className="text-sm text-white/60 italic">TBD</div>
            );
        }

        return (
            <div className={`text-sm ${isWinner ? "text-green-300 font-bold" : "text-white"}`}>
                <div className="font-bold">
                    {player.displayName} {isWinner ? "✓" : ""}
                </div>
                <div className="text-xs text-white/70">
                    Seed {player.seed} &middot; Losses {player.losses}
                    {player.eliminated ? " · Eliminated" : ""}
                </div>
            </div>
        );
    };

    const playerOneWon = match.winnerId != null && match.winnerId === match.playerOneId;
    const playerTwoWon = match.winnerId != null && match.winnerId === match.playerTwoId;

    return (
        <motion.div
            ref={popoverRef}
            role="dialog"
            aria-label={`Match ${match.matchNumber} of round ${match.round} details`}
            initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, y: 4 }}
            animate={prefersReducedMotion ? { opacity: 1 } : { opacity: 1, y: 0 }}
            exit={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, y: 4 }}
            transition={{ duration: 0.15, ease: "easeOut" }}
            className="absolute z-20 bg-slate-900/95 border border-slate-600 rounded shadow-xl p-3 min-w-[180px] text-left"
            style={{
                left: `${pixelPosition.left}px`,
                top: `${pixelPosition.top + 28}px`,
                transform: "translateX(-50%)",
            }}
        >
            <p className="text-xs uppercase tracking-wider text-yellow-300 font-bold">
                Round {match.round} · Match {match.matchNumber}
            </p>
            <div className="mt-2 flex flex-col gap-2">
                {renderPlayer(one, playerOneWon)}
                <div className="border-t border-slate-700" />
                {renderPlayer(two, playerTwoWon)}
            </div>
            <p className="mt-2 text-xs text-white/60">
                Status: {match.status}
            </p>
        </motion.div>
    );
};

export default BracketMatchDetailPopover;
