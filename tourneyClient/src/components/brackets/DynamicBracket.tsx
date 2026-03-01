import { FC, ReactElement } from "react";
import { motion } from "framer-motion";
import drawService from "@/services/drawService";
import { useWindowSize } from "@/hooks/useWindowSize";

type Props = {
    numPlayers: number;
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

const renderBracketLines = (
    rounds: MatchNode[][],
    keyPrefix: string,
    stroke: string,
    startDelayIndex: number
): { lines: ReactElement[]; nextDelayIndex: number } =>
{
    const lines: ReactElement[] = [];
    let delayCursor = startDelayIndex;

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
                lines.push(
                    <motion.line
                        key={`${keyPrefix}-h1-${roundIndex}-${targetIndex}-${sourceLocalIndex}`}
                        x1={source.x}
                        y1={source.y}
                        x2={elbowX}
                        y2={source.y}
                        stroke={stroke}
                        strokeWidth="2"
                        variants={drawService}
                        custom={delayCursor / 8}
                    />
                );
                delayCursor += 1;
            });

            if (mappedSources.length > 1)
            {
                const sortedByY = [...mappedSources].sort((a, b) => a.y - b.y);

                lines.push(
                    <motion.line
                        key={`${keyPrefix}-v-${roundIndex}-${targetIndex}`}
                        x1={elbowX}
                        y1={sortedByY[0].y}
                        x2={elbowX}
                        y2={sortedByY[sortedByY.length - 1].y}
                        stroke={stroke}
                        strokeWidth="2"
                        variants={drawService}
                        custom={delayCursor / 8}
                    />
                );
                delayCursor += 1;
            }

            lines.push(
                <motion.line
                    key={`${keyPrefix}-h2-${roundIndex}-${targetIndex}`}
                    x1={elbowX}
                    y1={target.y}
                    x2={target.x}
                    y2={target.y}
                    stroke={stroke}
                    strokeWidth="2"
                    variants={drawService}
                    custom={delayCursor / 8}
                />
            );
            delayCursor += 1;
        }
    }

    return { lines, nextDelayIndex: delayCursor };
};

const DrawDoubleEliminationBracket: FC<Props> = ({ numPlayers }) =>
{
    const { width, height } = useWindowSize();
    const safeWidth = Math.max(900, width);
    const safeHeight = Math.max(700, height);
    const bracketPlayers = clampToBracketSize(numPlayers);

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
                strokeWidth="2"
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
                strokeWidth="2"
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
                strokeWidth="2"
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
                strokeWidth="2"
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
                strokeWidth="2"
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
                strokeWidth="2"
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
        </motion.svg>
    );
};

export default DrawDoubleEliminationBracket;