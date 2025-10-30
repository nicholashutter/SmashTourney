import React, { JSX, use } from "react";
import { motion } from "framer-motion";
import drawService from "../../services/DrawService";
import { useWindowSize } from "@/hooks/useWindowSize";

type Props = {
    numPlayers: number;
};

const DrawWinnersBracket: React.FC<Props> = ({ numPlayers }) =>
{

    const {width, height} = useWindowSize();
    //define initial layout grid

    const viewBox = `0 0 ${width} ${height}`;

    // Starting horizontal position for the first column of match nodes
    const initialXOffset = width * .05;

    // Starting vertical position for the first match node
    const initialYOffset = height * .05;

    // Horizontal length of the connector line between rounds
    const matchConnectorWidth = width * .05;

    // Vertical height allocated per match node
    const matchBoxHeight = height * .04;

    // Horizontal spacing between each round of matches
    const matchHorizontalGap = width * .08;

    // Vertical spacing between match nodes within the same round
    const matchVerticalGap = height * .02;

    //matchnode is the visual representation of each match
    type MatchNode = {
        x: number;
        y: number;
        id: string;
    };

    //define initial node based on numPlayers prop
    let currentMatch: MatchNode[] = Array.from({ length: numPlayers }, (_, i) => ({
        x: initialXOffset,
        y: initialYOffset + i * (matchBoxHeight + matchVerticalGap),
        id: `C0-${i}`,
    }));

    //container for the bracket as recursively rendered
    const finalBracket: JSX.Element[] = [];

    // Round counter to track progression through bracket layers
    let roundIndex = 0;

    // Recursive bracket construction: pair match nodes until one remains
    while (currentMatch.length > 1)
    {
        const nextMatch: MatchNode[] = []; // Stores match nodes for the next round

        for (let i = 0; i < currentMatch.length - 1; i += 2)
        {
            const playerOne = currentMatch[i];
            const playerTwo = currentMatch[i + 1];

            // Vertical midpoint between paired nodes
            const verticalMidpoint = (playerOne.y + playerTwo.y) / 2;

            // Horizontal position for next round's connector
            const nextX = playerOne.x + matchHorizontalGap;

            // Bracket connector lines are pushed here
            finalBracket.push(
                <React.Fragment key={`C-${roundIndex}-${i}`}>

                    <motion.line
                        x1={playerOne.x}
                        y1={playerOne.y}
                        x2={nextX}
                        y2={playerOne.y}
                        stroke="#ffffff"
                        strokeWidth="2"
                        variants={drawService}
                        custom={roundIndex * 100 + i * 5}
                    />

                    <motion.line
                        x1={playerTwo.x}
                        y1={playerTwo.y}
                        x2={nextX}
                        y2={playerTwo.y}
                        stroke="#ffffff"
                        strokeWidth="2"
                        variants={drawService}
                        custom={roundIndex * 100 + i * 5 + 1}
                    />

                    <motion.line
                        x1={nextX}
                        y1={playerOne.y}
                        x2={nextX}
                        y2={playerTwo.y}
                        stroke="#ffffff"
                        strokeWidth="2"
                        variants={drawService}
                        custom={roundIndex * 100 + i * 5 + 2}
                    />

                    <motion.line
                        x1={nextX}
                        y1={verticalMidpoint}
                        x2={nextX + matchConnectorWidth}
                        y2={verticalMidpoint}
                        stroke="#ffffff"
                        strokeWidth="2"
                        variants={drawService}
                        custom={roundIndex * 100 + i * 5 + 3}
                    />
                </React.Fragment>
            );

            nextMatch.push({
                x: nextX + matchConnectorWidth,
                y: verticalMidpoint,
                id: `C${roundIndex + 1}-${i / 2}`,
            });
        }

        // Handle unpaired match node (odd number of players in current round)
        // TILT should not be possible
        if (currentMatch.length % 2 === 1)
        {
            const unpairedMatchNode = currentMatch[currentMatch.length - 1];
            nextMatch.push({
                x: unpairedMatchNode.x + matchHorizontalGap + matchConnectorWidth,
                y: unpairedMatchNode.y,
                id: `C${roundIndex + 1}-orphan`,
            });
        }

        currentMatch = nextMatch;
        roundIndex++;
    }

    //calculate number of rounds for losers bracket
    const loserBracketRounds = 2 * (roundIndex - 1);
    console.log("Loser bracket rounds:", loserBracketRounds);
    return (
        <motion.svg width="100%" height="100%" viewBox={viewBox} preserveAspectRatio="xMidYMid meet">
            {finalBracket}
        </motion.svg>
    );
};

export default DrawWinnersBracket