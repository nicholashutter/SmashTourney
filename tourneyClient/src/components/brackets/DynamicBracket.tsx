import { FC, ReactElement, useEffect, useMemo, useRef } from "react";
import { motion, useReducedMotion } from "framer-motion";
import { drawService } from "@/services/drawService";
import { useWindowSize } from "@/hooks/useWindowSize";
import { useBracketMotionConfig } from "@/hooks/useBracketMotionConfig";
import
{
    BracketLane,
    BracketMatchView,
    BracketPlayerView,
    BracketSnapshotResponse,
} from "@/models/entities/Bracket";

type Props = {
    snapshot: BracketSnapshotResponse | null;
    currentMatchId?: string | null;
    // The id of the player viewing this bracket. When a match contains this
    // player, the match gets a "YOU" badge and a coloured inner stroke so it
    // stands out from the other matches on the panel.
    viewerPlayerId?: string | null;
    // Fires when the user focuses a match (hover, click, or keyboard focus).
    // The bracket supplies the match and the match's viewBox-space position
    // so the parent can render an HTML popover next to the SVG.
    onMatchFocus?: (match: BracketMatchView, position: { x: number; y: number }) => void;
    // Fires when the user clears the focus (mouseleave, blur, or Escape).
    onMatchBlur?: () => void;
};

type PositionedMatch = {
    match: BracketMatchView;
    x: number;
    y: number;
    round: number;
};

type Region = {
    x: number;
    y: number;
    width: number;
    height: number;
};

// Width and height of one drawn match box, in viewBox units.
const MATCH_WIDTH = 132;
const MATCH_HEIGHT = 40;

// Inner-stroke inset so the "you" ring sits inside the match box without
// drawing over the outer border. One viewBox unit is roughly one screen pixel
// at the default safe width, so 1.5 keeps the badge crisp on small phones.
const YOU_INSET = 1.5;

// Colour used for the "YOU" badge. Kept in code rather than CSS so it lives
// next to the rest of the bracket's viewBox math.
const YOU_TAG_DIM_COLOR = "#475569";
const YOU_TEXT_COLOR = "#0f172a";

// Time between staggered columns during a new-round reveal. Pulled from the
// motion config at runtime so reduced-motion callers get a 0 s stagger.
const DROP_CONNECTOR_BASE_DELAY = 0.4;

// Groups matches into columns by round, ordered by match number within each.
//
// The API assigns round numbers, so this reflects the real bracket rather than
// a shape inferred from a player count.
const buildRoundColumns = (matches: BracketMatchView[]): BracketMatchView[][] =>
{
    const rounds = new Map<number, BracketMatchView[]>();

    for (const match of matches)
    {
        const existing = rounds.get(match.round);

        if (existing)
        {
            existing.push(match);
        }
        else
        {
            rounds.set(match.round, [match]);
        }
    }

    return Array.from(rounds.keys())
        .sort((left, right) => left - right)
        .map((round) => rounds.get(round)!.sort((left, right) => left.matchNumber - right.matchNumber));
};

// Assigns a screen position to every match in a lane.
const positionLane = (columns: BracketMatchView[][], region: Region): PositionedMatch[] =>
{
    if (columns.length === 0)
    {
        return [];
    }

    const columnStep = region.width / columns.length;

    return columns.flatMap((column, columnIndex) =>
    {
        const verticalStep = region.height / (column.length + 1);

        return column.map((match, matchIndex) => ({
            match,
            round: match.round,
            x: region.x + columnStep * columnIndex,
            y: region.y + verticalStep * (matchIndex + 1),
        }));
    });
};

// Resolves a participant's display name, or a placeholder when undecided.
//
// A match that has not been reached yet carries no player ids at all, which is
// exactly what the fixed bracket tree makes possible to draw.
const resolveName = (
    playerId: string | undefined,
    playersById: Map<string, BracketPlayerView>
): string =>
{
    if (!playerId)
    {
        return "TBD";
    }

    return playersById.get(playerId)?.displayName ?? "TBD";
};

// Reports whether a participant slot holds a synthetic bye.
//
// The API pads brackets to a power of two with placeholder players named
// "BYE n", and resolves those matches server-side. Drawing them the same as
// real players would imply someone won a game they never played.
const isByeName = (name: string): boolean => name.startsWith("BYE");

// Picks a colour for the "YOU" inner stroke that contrasts with the match
// box fill but is distinct from the LIVE yellow used elsewhere on the active
// match. A real player id is the only way to derive a stable colour without
// extra props the API does not send.
const viewerAccentColor = (playerId: string | null | undefined): string =>
{
    if (!playerId)
    {
        return "#facc15";
    }

    // Simple deterministic hash over the playerId. Smash colours are an
    // established palette in the fighter tile code; here we just need a colour
    // that does not collide with the LIVE yellow on the same match.
    const palette = ["#22d3ee", "#a78bfa", "#f472b6", "#34d399", "#fb923c", "#f87171"];
    let hash = 0;
    for (let index = 0; index < playerId.length; index++)
    {
        hash = (hash * 31 + playerId.charCodeAt(index)) % 100000;
    }
    return palette[hash % palette.length];
};

// Decides whether the viewer is one of the participants on this match.
const matchContainsViewer = (
    match: BracketMatchView,
    viewerPlayerId: string | null | undefined
): boolean =>
{
    if (!viewerPlayerId)
    {
        return false;
    }

    return match.playerOneId === viewerPlayerId || match.playerTwoId === viewerPlayerId;
};

// Renders one match box with both participants and their outcome.
const renderMatch = (
    positioned: PositionedMatch,
    playersById: Map<string, BracketPlayerView>,
    currentMatchId: string | null | undefined,
    viewerPlayerId: string | null | undefined,
    matchFadeInSeconds: number,
    onMatchFocus: Props["onMatchFocus"],
    onMatchBlur: Props["onMatchBlur"]
): ReactElement =>
{
    const { match, x, y } = positioned;
    const isCurrent = currentMatchId != null && match.matchId === currentMatchId;

    const playerOneName = resolveName(match.playerOneId, playersById);
    const playerTwoName = resolveName(match.playerTwoId, playersById);

    const playerOneWon = match.winnerId != null && match.winnerId === match.playerOneId;
    const playerTwoWon = match.winnerId != null && match.winnerId === match.playerTwoId;
    const isDecided = match.status === "COMPLETE";

    const containsViewer = matchContainsViewer(match, viewerPlayerId);
    const viewerEliminated = containsViewer
        && isDecided
        && match.winnerId !== viewerPlayerId;

    // A decided match dims the loser rather than hiding them, so the path a
    // player took through the bracket stays readable after they are out.
    const rowFill = (won: boolean, name: string): string =>
    {
        if (isByeName(name))
        {
            return "#64748b";
        }

        if (!isDecided)
        {
            return "#f8fafc";
        }

        return won ? "#4ade80" : "#94a3b8";
    };

    const playerOneIsViewer = match.playerOneId != null && match.playerOneId === viewerPlayerId;
    const playerTwoIsViewer = match.playerTwoId != null && match.playerTwoId === viewerPlayerId;
    const accentColor = viewerAccentColor(viewerPlayerId);

    // The match group is a focusable <g> so keyboard users can tab through the
    // bracket. The handlers are no-op when no focus callbacks are provided,
    // so existing call sites that do not pass them still work as pure SVG.
    const handleFocus = () =>
    {
        onMatchFocus?.(match, { x, y });
    };
    const handleBlur = () =>
    {
        onMatchBlur?.();
    };
    const handleClick = () =>
    {
        onMatchFocus?.(match, { x, y });
    };
    const handleKey = (event: React.KeyboardEvent<SVGGElement>) =>
    {
        if (event.key === "Enter" || event.key === " ")
        {
            event.preventDefault();
            onMatchFocus?.(match, { x, y });
        }
        if (event.key === "Escape")
        {
            onMatchBlur?.();
        }
    };

    return (
        <motion.g
            key={match.matchId}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: matchFadeInSeconds }}
            tabIndex={onMatchFocus ? 0 : -1}
            role={onMatchFocus ? "button" : undefined}
            aria-label={
                onMatchFocus
                    ? `Match ${match.matchNumber} of round ${match.round}: ${playerOneName} vs ${playerTwoName}`
                    : undefined
            }
            onMouseEnter={onMatchFocus ? handleFocus : undefined}
            onMouseLeave={onMatchBlur ? handleBlur : undefined}
            onClick={onMatchFocus ? handleClick : undefined}
            onFocus={onMatchFocus ? handleFocus : undefined}
            onBlur={onMatchBlur ? handleBlur : undefined}
            onKeyDown={onMatchFocus ? handleKey : undefined}
            style={{ cursor: onMatchFocus ? "pointer" : undefined }}
        >
            <rect
                x={x}
                y={y - MATCH_HEIGHT / 2}
                width={MATCH_WIDTH}
                height={MATCH_HEIGHT}
                rx="4"
                fill="#0f172a"
                fillOpacity={isCurrent ? 0.95 : 0.7}
                stroke={isCurrent ? "#facc15" : "#475569"}
                strokeWidth={isCurrent ? 2.5 : 1}
            />

            {containsViewer && !viewerEliminated && (
                <rect
                    x={x + YOU_INSET}
                    y={y - MATCH_HEIGHT / 2 + YOU_INSET}
                    width={MATCH_WIDTH - YOU_INSET * 2}
                    height={MATCH_HEIGHT - YOU_INSET * 2}
                    rx="3"
                    fill="none"
                    stroke={viewerEliminated ? YOU_TAG_DIM_COLOR : accentColor}
                    strokeWidth="1"
                    strokeDasharray="3 2"
                    pointerEvents="none"
                />
            )}

            <line
                x1={x}
                y1={y}
                x2={x + MATCH_WIDTH}
                y2={y}
                stroke="#334155"
                strokeWidth="1"
            />

            <text
                x={x + 7}
                y={y - 6}
                fill={rowFill(playerOneWon, playerOneName)}
                fontSize="12"
                fontWeight={
                    playerOneIsViewer && !viewerEliminated
                        ? "700"
                        : playerOneWon
                            ? "700"
                            : "500"
                }
            >
                {playerOneName}
            </text>

            <text
                x={x + 7}
                y={y + 15}
                fill={rowFill(playerTwoWon, playerTwoName)}
                fontSize="12"
                fontWeight={
                    playerTwoIsViewer && !viewerEliminated
                        ? "700"
                        : playerTwoWon
                            ? "700"
                            : "500"
                }
            >
                {playerTwoName}
            </text>

            {containsViewer && !viewerEliminated && (
                <g pointerEvents="none">
                    <rect
                        x={x + MATCH_WIDTH - 28}
                        y={y - MATCH_HEIGHT / 2 + 2}
                        width={24}
                        height={10}
                        rx={2}
                        fill={viewerEliminated ? YOU_TAG_DIM_COLOR : accentColor}
                        opacity={viewerEliminated ? 0.6 : 0.9}
                    />
                    <text
                        x={x + MATCH_WIDTH - 16}
                        y={y - MATCH_HEIGHT / 2 + 10}
                        textAnchor="middle"
                        fontSize="8"
                        fontWeight="700"
                        fill={YOU_TEXT_COLOR}
                    >
                        YOU
                    </text>
                    <title>
                        {viewerEliminated
                            ? "You were eliminated in this match"
                            : "You are in this match"}
                    </title>
                </g>
            )}
        </motion.g>
    );
};

// Draws an elbow connector from a match to the match its winner advances into.
//
// Every segment comes from nextMatchForWinner, so the drawn bracket is the
// bracket the server is actually running. Nothing here infers structure.
const renderConnectors = (
    positionedMatches: PositionedMatch[],
    positionsById: Map<string, PositionedMatch>
): ReactElement[] =>
{
    const connectors: ReactElement[] = [];

    positionedMatches.forEach((positioned) =>
    {
        const target = positioned.match.nextMatchForWinner
            ? positionsById.get(positioned.match.nextMatchForWinner)
            : undefined;

        if (!target)
        {
            return;
        }

        const startX = positioned.x + MATCH_WIDTH;
        const startY = positioned.y;
        const endX = target.x;
        const endY = target.y;
        const elbowX = startX + (endX - startX) / 2;
        // Round-staggered delay: a round-2 line waits for round 1 to finish
        // its draw before starting, so the bracket reads top-down rather than
        // all lines animating in at once.
        const delay = positioned.match.round * 0.35;

        connectors.push(
            <motion.polyline
                key={`connector-${positioned.match.matchId}`}
                points={`${startX},${startY} ${elbowX},${startY} ${elbowX},${endY} ${endX},${endY}`}
                fill="none"
                stroke="#64748b"
                strokeWidth="2"
                variants={drawService}
                custom={delay}
            />
        );
    });

    return connectors;
};

// Draws the double-elimination "drop" connector from a winners match to the
// losers match that the loser is sent to. Returns an empty list when no
// match has a `nextMatchForLoser` set, which is the case for single-elim
// tournaments.
//
// The polyline goes:
//   source match right edge -> drop channel (between the two regions)
//   -> over to the target's x -> down into the target's top edge
const renderDropConnectors = (
    positionedMatches: PositionedMatch[],
    positionsById: Map<string, PositionedMatch>,
    winnersRegion: Region,
    losersRegion: Region,
    safeHeight: number
): ReactElement[] =>
{
    const dropChannelY = (winnersRegion.y + winnersRegion.height + losersRegion.y) / 2;
    const connectors: ReactElement[] = [];

    positionedMatches.forEach((positioned) =>
    {
        if (!positioned.match.nextMatchForLoser)
        {
            return;
        }

        const target = positionsById.get(positioned.match.nextMatchForLoser);
        if (!target)
        {
            return;
        }

        // Skip drop connectors that do not actually move between lanes — the
        // API sometimes routes the GRAND_FINALS_RESET to itself, and that
        // would draw a loop on top of the winners connector.
        if (positioned.match.lane === target.match.lane
            && positioned.match.nextMatchForLoser === positioned.match.matchId)
        {
            return;
        }

        const startX = positioned.x + MATCH_WIDTH / 2;
        const startY = positioned.y + MATCH_HEIGHT / 2;
        const endX = target.x + MATCH_WIDTH / 2;
        const endY = target.y - MATCH_HEIGHT / 2;
        // Two elbows: down to the drop channel, across, then down into the
        // target. The vertical "in" segment is short on purpose so the eye
        // follows the line into the losers lane rather than off the canvas.
        const inX = endX;
        const downY = dropChannelY;
        const delay = (positioned.match.round * 0.35) + DROP_CONNECTOR_BASE_DELAY;

        const points = [
            `${startX},${startY}`,
            `${startX},${downY}`,
            `${inX},${downY}`,
            `${inX},${endY}`,
        ].join(" ");

        connectors.push(
            <motion.polyline
                key={`drop-connector-${positioned.match.matchId}`}
                points={points}
                fill="none"
                stroke="#f97316"
                strokeWidth="2"
                strokeDasharray="6 4"
                variants={drawService}
                custom={delay}
            />
        );

        // Downward chevron at the target so the eye lands on the losers
        // match. The text is placed just outside the match box, above the
        // top edge.
        connectors.push(
            <text
                key={`drop-chevron-${positioned.match.matchId}`}
                x={inX}
                y={endY - 3}
                textAnchor="middle"
                fontSize="9"
                fontWeight="700"
                fill="#f97316"
                pointerEvents="none"
            >
                ▼
            </text>
        );
    });

    // Reference parameter kept for future use (e.g. debug overlay or fallback
    // drop-channel y). eslint can't see why the safeHeight argument exists
    // for a future tweak, but the parameter is part of the public signature
    // so the call site doesn't have to know the implementation detail.
    void safeHeight;

    return connectors;
};

// Renders the live tournament bracket from the authoritative snapshot.
//
// Previously this drew a bracket-shaped diagram from a player count alone, so
// it could not show who had won, whose match was live, or who was out. It now
// renders the matches the API reports, which is the only way those can appear.
const DynamicBracket: FC<Props> = ({
    snapshot,
    currentMatchId,
    viewerPlayerId,
    onMatchFocus,
    onMatchBlur,
}) =>
{
    const { width, height } = useWindowSize();
    const safeWidth = Math.max(640, width || 640);
    const safeHeight = Math.max(420, height || 420);
    const motionConfig = useBracketMotionConfig();
    // `useReducedMotion` is also pulled directly so the empty-state HTML
    // overlay can use it without rerunning the bracket motion config.
    const prefersReducedMotion = useReducedMotion();
    // Tracks the rounds that have been revealed at least once. The bracket
    // re-renders on every poll, and a "new" round is one that wasn't present
    // in the previous snapshot — the ref is the cheapest way to detect that
    // without pushing state down through every match.
    const previousRoundsRef = useRef<Set<number>>(new Set());

    // Updates the previous-rounds ref after every render so the next poll can
    // diff against the current set.
    useEffect(() =>
    {
        if (!snapshot)
        {
            previousRoundsRef.current = new Set();
            return;
        }

        const nextRounds = new Set(snapshot.matches.map((match) => match.round));
        previousRoundsRef.current = nextRounds;
    }, [snapshot]);

    // Resolves a debounce ref so the round-reveal stagger does not replay on
    // unrelated re-renders. The bracket polls every 2 s, and any re-render
    // that did not actually grow the round set should not retrigger the
    // staggered column reveal. The debounce window is small (50 ms) because
    // a real new round is always accompanied by a payload change.
    const lastRevealRef = useRef<{ rounds: Set<number>; at: number }>({
        rounds: new Set(),
        at: 0,
    });

    const newRounds = useMemo<Set<number>>(() =>
    {
        if (!snapshot)
        {
            return new Set();
        }
        const currentRounds = new Set(snapshot.matches.map((match) => match.round));
        const previousRounds = previousRoundsRef.current;
        const lastReveal = lastRevealRef.current;
        const now = Date.now();

        const justRevealed = new Set<number>();
        currentRounds.forEach((round) =>
        {
            if (!previousRounds.has(round))
            {
                justRevealed.add(round);
            }
        });

        // Debounce: if the same rounds were already revealed within the last
        // 50 ms (e.g. an unrelated parent re-render), treat them as
        // not-new. This guards against re-keying on every poll.
        if (justRevealed.size > 0 && now - lastReveal.at < 50)
        {
            const sameAsLast = Array.from(justRevealed).every((round) => lastReveal.rounds.has(round))
                && justRevealed.size === lastReveal.rounds.size;
            if (sameAsLast)
            {
                return new Set();
            }
        }

        if (justRevealed.size > 0)
        {
            lastRevealRef.current = { rounds: justRevealed, at: now };
        }

        return justRevealed;
    }, [snapshot]);

    if (!snapshot || snapshot.matches.length === 0)
    {
        // The HTML overlay replaces the SVG <text> placeholder so the empty
        // state can use the same spinner / shimmer styling as the in-match and
        // lobby pages. Keeping the overlay in HTML also means screen readers
        // announce it as a region, which the SVG text did not.
        return (
            <div className="flex items-center justify-center w-full h-full">
                <div className="flex flex-col items-center gap-3 text-white text-center">
                    <div className="flex items-center gap-2" aria-hidden="true">
                        <span className="w-3 h-3 rounded-full bg-yellow-300 animate-pulse [animation-delay:-0.3s]" />
                        <span className="w-3 h-3 rounded-full bg-yellow-300 animate-pulse [animation-delay:-0.15s]" />
                        <span className="w-3 h-3 rounded-full bg-yellow-300 animate-pulse" />
                    </div>
                    <p className="text-lg font-bold">Waiting for the bracket...</p>
                    <p className="text-sm text-white/70">The host will start the next round shortly.</p>
                </div>
            </div>
        );
    }

    const playersById = new Map(snapshot.players.map((player) => [player.playerId, player]));

    const matchesInLane = (lane: BracketLane): BracketMatchView[] =>
        snapshot.matches.filter((match) => match.lane === lane);

    const winnersMatches = matchesInLane("WINNERS");
    const losersMatches = matchesInLane("LOSERS");
    const finalsMatches = [...matchesInLane("GRAND_FINALS"), ...matchesInLane("GRAND_FINALS_RESET")];

    const isDoubleElimination = losersMatches.length > 0 || finalsMatches.length > 0;

    // Double elimination splits the canvas between the two lanes. Single
    // elimination has one lane and uses the full height.
    const winnersRegion: Region = {
        x: safeWidth * 0.03,
        y: safeHeight * (isDoubleElimination ? 0.09 : 0.06),
        width: safeWidth * (isDoubleElimination ? 0.78 : 0.94) - MATCH_WIDTH,
        height: safeHeight * (isDoubleElimination ? 0.38 : 0.88),
    };

    const losersRegion: Region = {
        x: safeWidth * 0.03,
        y: safeHeight * 0.55,
        width: safeWidth * 0.78 - MATCH_WIDTH,
        height: safeHeight * 0.36,
    };

    const finalsRegion: Region = {
        x: safeWidth * 0.84,
        y: safeHeight * 0.30,
        width: safeWidth * 0.12,
        height: safeHeight * 0.30,
    };

    const positioned = [
        ...positionLane(buildRoundColumns(winnersMatches), winnersRegion),
        ...positionLane(buildRoundColumns(losersMatches), losersRegion),
        ...positionLane(buildRoundColumns(finalsMatches), finalsRegion),
    ];

    const positionsById = new Map(positioned.map((entry) => [entry.match.matchId, entry]));

    // Group positioned matches by round for the new-round reveal stagger.
    const positionedByRound = new Map<number, PositionedMatch[]>();
    positioned.forEach((entry) =>
    {
        const bucket = positionedByRound.get(entry.round) ?? [];
        bucket.push(entry);
        positionedByRound.set(entry.round, bucket);
    });

    // The active-match LIVE tag is positioned just above the current match.
    const activeMatchPosition = currentMatchId
        ? positionsById.get(currentMatchId)
        : undefined;

    return (
        <motion.svg
            width="100%"
            height="100%"
            viewBox={`0 0 ${safeWidth} ${safeHeight}`}
            preserveAspectRatio="xMidYMid meet"
            initial="hidden"
            animate="visible"
            className="overflow-visible"
        >
            <defs>
                <filter id="bracket-live-glow" x="-20%" y="-50%" width="140%" height="200%">
                    <feGaussianBlur stdDeviation="3" result="blur" />
                    <feMerge>
                        <feMergeNode in="blur" />
                        <feMergeNode in="SourceGraphic" />
                    </feMerge>
                </filter>
            </defs>

            <text x={safeWidth * 0.03} y={safeHeight * 0.05} fill="#ffffff" fontSize="18" fontWeight="700">
                {isDoubleElimination ? "WINNERS BRACKET" : "BRACKET"}
            </text>

            {isDoubleElimination && (
                <text x={safeWidth * 0.03} y={safeHeight * 0.52} fill="#ffffff" fontSize="18" fontWeight="700">
                    LOSERS BRACKET
                </text>
            )}

            {finalsMatches.length > 0 && (
                <text x={safeWidth * 0.84} y={safeHeight * 0.26} fill="#ffffff" fontSize="15" fontWeight="700">
                    GRAND FINALS
                </text>
            )}

            {renderConnectors(positioned, positionsById)}
            {isDoubleElimination
                && renderDropConnectors(positioned, positionsById, winnersRegion, losersRegion, safeHeight)}

            {/* New-round reveal: wrap each round's column in a parent motion.g
                with a stagger so a new round "drops in" rather than appearing
                all at once. The wrapper element type stays the same across
                renders so the matches do not re-mount when the "is new" flag
                flips from true to false on a later poll. The `initial={false}`
                short-circuit prevents framer-motion from replaying the stagger
                on a round that was already revealed in an earlier snapshot. */}
            {Array.from(positionedByRound.entries()).map(([round, roundEntries]) =>
            {
                const isNew = newRounds.has(round);
                const shouldStagger = isNew && !prefersReducedMotion;

                return (
                    <motion.g
                        key={`round-${round}`}
                        initial={shouldStagger ? "hidden" : false}
                        animate="visible"
                        variants={shouldStagger
                            ? {
                                hidden: { opacity: 0 },
                                visible: {
                                    opacity: 1,
                                    transition: {
                                        staggerChildren: motionConfig.columnStaggerSeconds,
                                        delayChildren: 0.1,
                                    },
                                },
                            }
                            : undefined}
                    >
                        {roundEntries.map((entry) => (
                            <motion.g
                                key={entry.match.matchId}
                                initial={shouldStagger ? "hidden" : false}
                                animate="visible"
                                variants={shouldStagger
                                    ? {
                                        hidden: { opacity: 0, y: -12 },
                                        visible: { opacity: 1, y: 0 },
                                    }
                                    : undefined}
                            >
                                {renderMatch(entry, playersById, currentMatchId, viewerPlayerId, motionConfig.matchFadeInSeconds, onMatchFocus, onMatchBlur)}
                            </motion.g>
                        ))}
                    </motion.g>
                );
            })}

            {activeMatchPosition && !prefersReducedMotion && (
                <g pointerEvents="none">
                    {/* Outer breathing glow — the wider, blurred halo that
                        signals "this is the live match" at a glance. */}
                    <rect
                        x={activeMatchPosition.x - 1.5}
                        y={activeMatchPosition.y - MATCH_HEIGHT / 2 - 1.5}
                        width={MATCH_WIDTH + 3}
                        height={MATCH_HEIGHT + 3}
                        rx="5"
                        fill="none"
                        stroke="#facc15"
                        strokeOpacity="0.35"
                        strokeWidth="6"
                        filter="url(#bracket-live-glow)"
                        className="bracket-live-glow"
                    />
                    {/* The pulsing stroke on the match itself. */}
                    <rect
                        x={activeMatchPosition.x}
                        y={activeMatchPosition.y - MATCH_HEIGHT / 2}
                        width={MATCH_WIDTH}
                        height={MATCH_HEIGHT}
                        rx="4"
                        fill="none"
                        stroke="#facc15"
                        strokeWidth="2.5"
                        className="bracket-live-stroke"
                    />
                    {/* LIVE tag above the match. */}
                    <g>
                        <rect
                            x={activeMatchPosition.x + MATCH_WIDTH / 2 - 18}
                            y={activeMatchPosition.y - MATCH_HEIGHT / 2 - 14}
                            width={36}
                            height={12}
                            rx={2}
                            fill="#facc15"
                            fillOpacity="0.85"
                        />
                        <text
                            x={activeMatchPosition.x + MATCH_WIDTH / 2}
                            y={activeMatchPosition.y - MATCH_HEIGHT / 2 - 5}
                            textAnchor="middle"
                            fontSize="9"
                            fontWeight="800"
                            fill="#0f172a"
                        >
                            LIVE
                        </text>
                    </g>
                </g>
            )}

            {activeMatchPosition && prefersReducedMotion && (
                <g pointerEvents="none">
                    <rect
                        x={activeMatchPosition.x}
                        y={activeMatchPosition.y - MATCH_HEIGHT / 2}
                        width={MATCH_WIDTH}
                        height={MATCH_HEIGHT}
                        rx="4"
                        fill="none"
                        stroke="#facc15"
                        strokeWidth="2.5"
                    />
                    <g>
                        <rect
                            x={activeMatchPosition.x + MATCH_WIDTH / 2 - 18}
                            y={activeMatchPosition.y - MATCH_HEIGHT / 2 - 14}
                            width={36}
                            height={12}
                            rx={2}
                            fill="#facc15"
                            fillOpacity="0.85"
                        />
                        <text
                            x={activeMatchPosition.x + MATCH_WIDTH / 2}
                            y={activeMatchPosition.y - MATCH_HEIGHT / 2 - 5}
                            textAnchor="middle"
                            fontSize="9"
                            fontWeight="800"
                            fill="#0f172a"
                        >
                            LIVE
                        </text>
                    </g>
                </g>
            )}
        </motion.svg>
    );
};

export default DynamicBracket;
