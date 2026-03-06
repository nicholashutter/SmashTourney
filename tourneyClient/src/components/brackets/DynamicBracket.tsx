import { FC, ReactElement } from "react";
import { motion } from "framer-motion";
import { drawService } from "@/services/drawService";
import { useWindowSize } from "@/hooks/useWindowSize";

type Props = {
    numPlayers: number;
    playerNames?: string[];
    mode?: "SINGLE_ELIMINATION" | "DOUBLE_ELIMINATION";
};

type MatchNode = {
    x: number;
    y: number;
};

type Region = {
    x: number;
    y: number;
    width: number;
    height: number;
};

// Normalizes requested player count to the nearest supported power-of-two bracket size.
const clampToBracketSize = (requestedPlayers: number): number =>
{
    const sanitized = Math.max(2, Math.floor(requestedPlayers || 2));
    let powerOfTwo = 1;

    while (powerOfTwo < sanitized)
    {
        powerOfTwo *= 2;
    }

    return powerOfTwo;
};

// Builds the number of matches per winners round from the opening match count.
const getRoundCounts = (startingMatches: number): number[] =>
{
    const counts: number[] = [];
    let currentCount = Math.max(1, startingMatches);

    while (currentCount >= 1)
    {
        counts.push(currentCount);

        if (currentCount === 1)
        {
            break;
        }

        currentCount = Math.max(1, Math.floor(currentCount / 2));
    }

    return counts;
};

// Builds the number of matches per losers round for double-elimination layout.
const getLosersRoundCounts = (bracketPlayers: number): number[] =>
{
    const winnersRounds = Math.log2(bracketPlayers);

    if (winnersRounds <= 1)
    {
        return [1];
    }

    const counts: number[] = [];

    for (let stage = 0; stage < winnersRounds - 1; stage++)
    {
        const count = Math.max(1, Math.floor(bracketPlayers / Math.pow(2, stage + 2)));
        counts.push(count, count);
    }

    return counts;
};

// Converts round match counts into x/y coordinates for bracket node rendering.
const buildRoundNodes = (roundCounts: number[], region: Region): MatchNode[][] =>
{
    const totalRounds = roundCounts.length;
    const columnGap = region.width / Math.max(2, totalRounds + 0.6);
    const startX = region.x + columnGap * 0.35;

    return roundCounts.map((count, roundIndex) =>
    {
        const x = startX + columnGap * roundIndex;
        const verticalStep = region.height / (count + 1);

        return Array.from({ length: count }, (_, index) => ({
            x,
            y: region.y + verticalStep * (index + 1),
        }));
    });
};

// Draws connector lines between bracket rounds and returns animation cursor progress.
const renderBracketLines = (
    rounds: MatchNode[][],
    keyPrefix: string,
    stroke: string,
    startDelayIndex: number
): { lines: ReactElement[]; nextDelayIndex: number } =>
{
    const lines: ReactElement[] = [];
    let delayCursor = startDelayIndex;
    const accentStroke = "#020617";
    const accentWidth = "6";
    const mainWidth = "3.2";

    const pushSegment = (key: string, x1: number, y1: number, x2: number, y2: number, delay: number) =>
    {
        lines.push(
            <motion.line
                key={`${key}-accent`}
                x1={x1}
                y1={y1}
                x2={x2}
                y2={y2}
                stroke={accentStroke}
                strokeOpacity="0.55"
                strokeWidth={accentWidth}
                variants={drawService}
                custom={delay / 8}
            />
        );

        lines.push(
            <motion.line
                key={`${key}-main`}
                x1={x1}
                y1={y1}
                x2={x2}
                y2={y2}
                stroke={stroke}
                strokeWidth={mainWidth}
                variants={drawService}
                custom={delay / 8}
            />
        );
    };

    for (let roundIndex = 0; roundIndex < rounds.length - 1; roundIndex++)
    {
        const currentRound = rounds[roundIndex];
        const nextRound = rounds[roundIndex + 1];

        if (currentRound.length === 0 || nextRound.length === 0)
        {
            continue;
        }

        const elbowX = currentRound[0].x + (nextRound[0].x - currentRound[0].x) * 0.58;

        for (let targetIndex = 0; targetIndex < nextRound.length; targetIndex++)
        {
            const target = nextRound[targetIndex];

            const mappedSources = currentRound.filter((_, sourceIndex) =>
                Math.floor((sourceIndex * nextRound.length) / currentRound.length) === targetIndex
            );

            mappedSources.forEach((source, sourceLocalIndex) =>
            {
                pushSegment(
                    `${keyPrefix}-h1-${roundIndex}-${targetIndex}-${sourceLocalIndex}`,
                    source.x,
                    source.y,
                    elbowX,
                    source.y,
                    delayCursor
                );
                delayCursor += 1;
            });

            if (mappedSources.length > 1)
            {
                const sortedByY = [...mappedSources].sort((a, b) => a.y - b.y);

                pushSegment(
                    `${keyPrefix}-v-${roundIndex}-${targetIndex}`,
                    elbowX,
                    sortedByY[0].y,
                    elbowX,
                    sortedByY[sortedByY.length - 1].y,
                    delayCursor
                );
                delayCursor += 1;
            }

            pushSegment(
                `${keyPrefix}-h2-${roundIndex}-${targetIndex}`,
                elbowX,
                target.y,
                target.x,
                target.y,
                delayCursor
            );
            delayCursor += 1;
        }
    }

    return { lines, nextDelayIndex: delayCursor };
};

// Renders the tournament bracket skeleton and overlays participant names on opening spokes.
const DrawDoubleEliminationBracket: FC<Props> = ({ numPlayers, playerNames = [], mode = "DOUBLE_ELIMINATION" }) =>
{
    const { width, height } = useWindowSize();
    const safeWidth = Math.max(900, width);
    const safeHeight = Math.max(700, height);
    const bracketPlayers = clampToBracketSize(numPlayers);
    const isSingleElimination = mode === "SINGLE_ELIMINATION";

    if (isSingleElimination)
    {
        const singleRoundCounts = getRoundCounts(bracketPlayers / 2);
        const singleRegion: Region = {
            x: safeWidth * 0.04,
            y: safeHeight * 0.1,
            width: safeWidth * 0.92,
            height: safeHeight * 0.8,
        };

        const singleRounds = buildRoundNodes(singleRoundCounts, singleRegion);
        const singleLinesResult = renderBracketLines(singleRounds, "single", "#ffffff", 0);

        const openingRoundNodes = singleRounds[0] ?? [];
        const openingRoundParticipantLabels = openingRoundNodes.flatMap((matchNode, matchIndex) =>
        {
            const firstParticipantIndex = matchIndex * 2;
            const secondParticipantIndex = firstParticipantIndex + 1;

            const participantOneName = playerNames[firstParticipantIndex] ?? "TBD";
            const participantTwoName = playerNames[secondParticipantIndex] ?? "TBD";

            return [
                {
                    key: `single-opening-participant-top-${matchIndex}`,
                    x: matchNode.x - 120,
                    y: matchNode.y - 10,
                    label: participantOneName,
                },
                {
                    key: `single-opening-participant-bottom-${matchIndex}`,
                    x: matchNode.x - 120,
                    y: matchNode.y + 14,
                    label: participantTwoName,
                },
            ];
        });

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
                <text x={safeWidth * 0.04} y={safeHeight * 0.06} fill="#ffffff" fontSize="20" fontWeight="700">
                    TOURNAMENT BRACKET
                </text>

                {singleLinesResult.lines}
                {openingRoundParticipantLabels.map((participantLabel) => (
                    <text
                        key={participantLabel.key}
                        x={participantLabel.x}
                        y={participantLabel.y}
                        fill="#ffffff"
                        stroke="#020617"
                        strokeOpacity="0.85"
                        strokeWidth="0.8"
                        paintOrder="stroke"
                        fontSize="13"
                        fontWeight="700"
                    >
                        {participantLabel.label}
                    </text>
                ))}
            </motion.svg>
        );
    }

    const winnersRoundCounts = getRoundCounts(bracketPlayers / 2);
    const losersRoundCounts = getLosersRoundCounts(bracketPlayers);

    const winnersRegion: Region = {
        x: safeWidth * 0.04,
        y: safeHeight * 0.08,
        width: safeWidth * 0.92,
        height: safeHeight * 0.38,
    };

    const losersRegion: Region = {
        x: safeWidth * 0.04,
        y: safeHeight * 0.54,
        width: safeWidth * 0.92,
        height: safeHeight * 0.36,
    };

    const winnersRounds = buildRoundNodes(winnersRoundCounts, winnersRegion);
    const losersRounds = buildRoundNodes(losersRoundCounts, losersRegion);

    const openingRoundNodes = winnersRounds[0] ?? [];
    const openingRoundParticipantLabels = openingRoundNodes.flatMap((matchNode, matchIndex) =>
    {
        const firstParticipantIndex = matchIndex * 2;
        const secondParticipantIndex = firstParticipantIndex + 1;

        const participantOneName = playerNames[firstParticipantIndex] ?? "TBD";
        const participantTwoName = playerNames[secondParticipantIndex] ?? "TBD";

        return [
            {
                key: `opening-participant-top-${matchIndex}`,
                x: matchNode.x - 120,
                y: matchNode.y - 10,
                label: participantOneName,
            },
            {
                key: `opening-participant-bottom-${matchIndex}`,
                x: matchNode.x - 120,
                y: matchNode.y + 14,
                label: participantTwoName,
            },
        ];
    });

    const winnersLinesResult = renderBracketLines(winnersRounds, "winners", "#ffffff", 0);
    const losersLinesResult = renderBracketLines(
        losersRounds,
        "losers",
        "#f5f5f5",
        winnersLinesResult.nextDelayIndex
    );

    const winnersFinal = winnersRounds[winnersRounds.length - 1]?.[0];
    const losersFinal = losersRounds[losersRounds.length - 1]?.[0];
    const grandFinalX = safeWidth * 0.94;
    const grandFinalY = safeHeight * 0.5;

    const grandFinalLines: ReactElement[] = [];
    let grandDelay = losersLinesResult.nextDelayIndex;

    if (winnersFinal)
    {
        const elbowX = winnersFinal.x + (grandFinalX - winnersFinal.x) * 0.5;
        grandFinalLines.push(
            <motion.line
                key="grand-w-h"
                x1={winnersFinal.x}
                y1={winnersFinal.y}
                x2={elbowX}
                y2={winnersFinal.y}
                stroke="#ffffff"
                strokeWidth="3.2"
                variants={drawService}
                custom={grandDelay / 8}
            />
        );
        grandDelay += 1;
        grandFinalLines.push(
            <motion.line
                key="grand-w-v"
                x1={elbowX}
                y1={winnersFinal.y}
                x2={elbowX}
                y2={grandFinalY}
                stroke="#ffffff"
                strokeWidth="3.2"
                variants={drawService}
                custom={grandDelay / 8}
            />
        );
        grandDelay += 1;
        grandFinalLines.push(
            <motion.line
                key="grand-w-to-final"
                x1={elbowX}
                y1={grandFinalY}
                x2={grandFinalX}
                y2={grandFinalY}
                stroke="#ffffff"
                strokeWidth="3.2"
                variants={drawService}
                custom={grandDelay / 8}
            />
        );
        grandDelay += 1;
    }

    if (losersFinal)
    {
        const elbowX = losersFinal.x + (grandFinalX - losersFinal.x) * 0.5;
        grandFinalLines.push(
            <motion.line
                key="grand-l-h"
                x1={losersFinal.x}
                y1={losersFinal.y}
                x2={elbowX}
                y2={losersFinal.y}
                stroke="#f5f5f5"
                strokeWidth="3.2"
                variants={drawService}
                custom={grandDelay / 8}
            />
        );
        grandDelay += 1;
        grandFinalLines.push(
            <motion.line
                key="grand-l-v"
                x1={elbowX}
                y1={losersFinal.y}
                x2={elbowX}
                y2={grandFinalY}
                stroke="#f5f5f5"
                strokeWidth="3.2"
                variants={drawService}
                custom={grandDelay / 8}
            />
        );
        grandDelay += 1;
        grandFinalLines.push(
            <motion.line
                key="grand-l-to-final"
                x1={elbowX}
                y1={grandFinalY}
                x2={grandFinalX}
                y2={grandFinalY}
                stroke="#f5f5f5"
                strokeWidth="3.2"
                variants={drawService}
                custom={grandDelay / 8}
            />
        );
    }

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
            <text x={safeWidth * 0.04} y={safeHeight * 0.05} fill="#ffffff" fontSize="20" fontWeight="700">
                WINNERS BRACKET
            </text>
            <text x={safeWidth * 0.04} y={safeHeight * 0.51} fill="#ffffff" fontSize="20" fontWeight="700">
                LOSERS BRACKET
            </text>
            <text x={safeWidth * 0.86} y={safeHeight * 0.49} fill="#ffffff" fontSize="16" fontWeight="700">
                GRAND FINALS
            </text>

            {winnersLinesResult.lines}
            {losersLinesResult.lines}
            {grandFinalLines}
            {openingRoundParticipantLabels.map((participantLabel) => (
                <text
                    key={participantLabel.key}
                    x={participantLabel.x}
                    y={participantLabel.y}
                    fill="#ffffff"
                    stroke="#020617"
                    strokeOpacity="0.85"
                    strokeWidth="0.8"
                    paintOrder="stroke"
                    fontSize="13"
                    fontWeight="700"
                >
                    {participantLabel.label}
                </text>
            ))}
        </motion.svg>
    );
};

export default DrawDoubleEliminationBracket;