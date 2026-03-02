import { useEffect, useState, ReactNode } from "react";
import { GameDataContext } from "@/components/GameDataContext";


//define and export the provider component for the gameDataContext object
export const GameDataProvider = ({ children }: { children: ReactNode }) => 
{
    const [gameId, setGameId] = useState<string | null>(() => localStorage.getItem("gameId"));
    const [playerId, setPlayerId] = useState<string | null>(() => localStorage.getItem("playerId"));
    const [gameStarted, setGameStarted] = useState<boolean>(() => localStorage.getItem("gameStarted") === "true");
    const [currentRoute, setCurrentRoute] = useState<string | null>(() => localStorage.getItem("currentRoute"));

    useEffect(() =>
    {
        if (gameId)
        {
            localStorage.setItem("gameId", gameId);
        }
        else
        {
            localStorage.removeItem("gameId");
        }
    }, [gameId]);

    useEffect(() =>
    {
        if (playerId)
        {
            localStorage.setItem("playerId", playerId);
        }
        else
        {
            localStorage.removeItem("playerId");
        }
    }, [playerId]);

    useEffect(() =>
    {
        localStorage.setItem("gameStarted", gameStarted ? "true" : "false");
    }, [gameStarted]);

    useEffect(() =>
    {
        if (currentRoute)
        {
            localStorage.setItem("currentRoute", currentRoute);
        }
        else
        {
            localStorage.removeItem("currentRoute");
        }
    }, [currentRoute]);

    return (
        <GameDataContext.Provider value=
            {
                {
                    gameId: gameId,
                    playerId: playerId,
                    gameStarted: gameStarted,
                    currentRoute: currentRoute,
                    setGameId: setGameId,
                    setPlayerId: setPlayerId,
                    setGameStarted: setGameStarted,
                    setCurrentRoute: setCurrentRoute,
                }
            }
        >
            {children}
        </GameDataContext.Provider>
    );
};