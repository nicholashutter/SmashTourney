import { useEffect, useState, ReactNode } from "react";
import { GameDataContext } from "@/components/GameDataContext";

// Provides persisted game session values to child routes.
export const GameDataProvider = ({ children }: { children: ReactNode }) =>
{
    const [gameId, setGameId] = useState<string | null>(sessionStorage.getItem("gameId"));
    const [playerId, setPlayerId] = useState<string | null>(sessionStorage.getItem("playerId"));
    const [isHost, setIsHost] = useState<boolean>(sessionStorage.getItem("isHost") === "true");
    const [gameStarted, setGameStarted] = useState<boolean>(sessionStorage.getItem("gameStarted") === "true");
    const [currentRoute, setCurrentRoute] = useState<string | null>(sessionStorage.getItem("currentRoute"));

    useEffect(() =>
    {
        if (gameId)
        {
            sessionStorage.setItem("gameId", gameId);
        }
        else
        {
            sessionStorage.removeItem("gameId");
        }
    }, [gameId]);

    useEffect(() =>
    {
        if (playerId)
        {
            sessionStorage.setItem("playerId", playerId);
        }
        else
        {
            sessionStorage.removeItem("playerId");
        }
    }, [playerId]);

    useEffect(() =>
    {
        sessionStorage.setItem("isHost", isHost ? "true" : "false");
    }, [isHost]);

    useEffect(() =>
    {
        sessionStorage.setItem("gameStarted", gameStarted ? "true" : "false");
    }, [gameStarted]);

    useEffect(() =>
    {
        if (currentRoute)
        {
            sessionStorage.setItem("currentRoute", currentRoute);
        }
        else
        {
            sessionStorage.removeItem("currentRoute");
        }
    }, [currentRoute]);

    return (
        <GameDataContext.Provider
            value={{
                gameId,
                playerId,
                isHost,
                gameStarted,
                currentRoute,
                setGameId,
                setPlayerId,
                setIsHost,
                setGameStarted,
                setCurrentRoute,
            }}
        >
            {children}
        </GameDataContext.Provider>
    );
};