import { FC, ReactElement } from "react";
import { motion } from "framer-motion";
import { drawService } from "@/services/drawService";
import { useWindowSize } from "@/hooks/useWindowSize";
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
};

type PositionedMatch = {
    match: BracketMatchView;
    x: number;
    y: number;
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

// Renders one match box with both participants and their outcome.
const renderMatch = (
    positioned: PositionedMatch,
    playersById: Map<string, BracketPlayerView>,
    currentMatchId: string | null | undefined
): ReactElement =>
{
    const { match, x, y } = positioned;
    const isCurrent = currentMatchId != null && match.matchId === currentMatchId;

    const playerOneName = resolveName(match.playerOneId, playersById);
    const playerTwoName = resolveName(match.playerTwoId, playersById);

    const playerOneWon = match.winnerId != null && match.winnerId === match.playerOneId;
    const playerTwoWon = match.winnerId != null && match.winnerId === match.playerTwoId;
    const isDecided = match.status === "COMPLETE";

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

    return (
        <motion.g
            key={match.matchId}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.35 }}
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
                fontWeight={playerOneWon ? "700" : "500"}
            >
                {playerOneName}
            </text>

            <text
                x={x + 7}
                y={y + 15}
                fill={rowFill(playerTwoWon, playerTwoName)}
                fontSize="12"
                fontWeight={playerTwoWon ? "700" : "500"}
            >
                {playerTwoName}
            </text>
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

    positionedMatches.forEach((positioned, index) =>
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
        const delay = index / 12;

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

// Renders the live tournament bracket from the authoritative snapshot.
//
// Previously this drew a bracket-shaped diagram from a player count alone, so
// it could not show who had won, whose match was live, or who was out. It now
// renders the matches the API reports, which is the only way those can appear.
const DynamicBracket: FC<Props> = ({ snapshot, currentMatchId }) =>
{
    const { width, height } = useWindowSize();
    const safeWidth = Math.max(640, width || 640);
    const safeHeight = Math.max(420, height || 420);

    if (!snapshot || snapshot.matches.length === 0)
    {
        return (
            <svg
                width="100%"
                height="100%"
                viewBox={`0 0 ${safeWidth} ${safeHeight}`}
                preserveAspectRatio="xMidYMid meet"
                className="overflow-visible"
            >
                <text
                    x={safeWidth / 2}
                    y={safeHeight / 2}
                    fill="#ffffff"
                    fontSize="20"
                    fontWeight="700"
                    textAnchor="middle"
                >
                    Waiting for the bracket...
                </text>
            </svg>
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
            {positioned.map((entry) => renderMatch(entry, playersById, currentMatchId))}
        </motion.svg>
    );
};

export default DynamicBracket;
