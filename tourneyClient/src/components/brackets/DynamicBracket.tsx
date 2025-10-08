import React from "react";
import { motion } from "framer-motion";
import drawService from "../../services/DrawService";

type Props = {
  numPlayers: number;
};

const DynamicBracket: React.FC<Props> = ({ numPlayers }) => {
  const startX = 40;
  const startY = 40;
  const cWidth = 60;
  const cHeight = 40;
  const roundSpacingX = 100;
  const verticalSpacing = 20;

  type CShape = {
    x: number;
    y: number;
    id: string;
  };

  // Initial C shapes
  let currentLayer: CShape[] = Array.from({ length: numPlayers }, (_, i) => ({
    x: startX,
    y: startY + i * (cHeight + verticalSpacing),
    id: `C0-${i}`,
  }));

  const allLines: JSX.Element[] = [];

  let round = 0;
  while (currentLayer.length > 1) {
    const nextLayer: CShape[] = [];

    for (let i = 0; i < currentLayer.length - 1; i += 2) {
      const top = currentLayer[i];
      const bottom = currentLayer[i + 1];

      const midY = (top.y + bottom.y) / 2;
      const nextX = top.x + roundSpacingX;

      allLines.push(
        <React.Fragment key={`C-${round}-${i}`}>
        
          <motion.line
            x1={top.x}
            y1={top.y}
            x2={nextX}
            y2={top.y}
            stroke="#ffffff"
            strokeWidth="2"
            variants={drawService}
            custom={round * 100 + i * 5}
          />
        
          <motion.line
            x1={bottom.x}
            y1={bottom.y}
            x2={nextX}
            y2={bottom.y}
            stroke="#ffffff"
            strokeWidth="2"
            variants={drawService}
            custom={round * 100 + i * 5 + 1}
          />
    
          <motion.line
            x1={nextX}
            y1={top.y}
            x2={nextX}
            y2={bottom.y}
            stroke="#ffffff"
            strokeWidth="2"
            variants={drawService}
            custom={round * 100 + i * 5 + 2}
          />
    
          <motion.line
            x1={nextX}
            y1={midY}
            x2={nextX + cWidth}
            y2={midY}
            stroke="#ffffff"
            strokeWidth="2"
            variants={drawService}
            custom={round * 100 + i * 5 + 3}
          />
        </React.Fragment>
      );

      nextLayer.push({
        x: nextX + cWidth,
        y: midY,
        id: `C${round + 1}-${i / 2}`,
      });
    }


    if (currentLayer.length % 2 === 1) {
      const orphan = currentLayer[currentLayer.length - 1];
      nextLayer.push({
        x: orphan.x + roundSpacingX + cWidth,
        y: orphan.y,
        id: `C${round + 1}-orphan`,
      });
    }

    currentLayer = nextLayer;
    round++;
  }

  const loserBracketRounds = 2 * (round - 1);
console.log("Loser bracket rounds:", loserBracketRounds);
  return (
    <motion.svg width="1200" height="1000" viewBox="0 0 1200 1000">
      {allLines}
    </motion.svg>
  );
};

export default DynamicBracket