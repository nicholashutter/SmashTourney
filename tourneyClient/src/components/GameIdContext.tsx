import { useState, ReactNode } from "react";
import { GameDataContext } from "@/components/GameDataContext";


//define and export the provider component for the gameDataContext object
export const GameDataProvider = ({ children }: { children: ReactNode }) => 
{
    const [gameId, setGameId] = useState<string | null>(null);
    const [playerId, setPlayerId] = useState<string | null>(null);

    return (
        <GameDataContext.Provider value=
            {
                {
                    gameId: gameId,
                    playerId: playerId,
                    setGameId: setGameId,
                    setPlayerId: setPlayerId,
                }
            }
        >
            {children}
        </GameDataContext.Provider>
    );
};