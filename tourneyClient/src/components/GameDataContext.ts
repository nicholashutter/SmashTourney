import { createContext } from "react";

export type GameData =
    {
        gameId: string | null;
        playerId: string | null;
        gameStarted: boolean;
        currentRoute: string | null;
        setGameId: (id: string) => void;
        setPlayerId: (id: string) => void;
        setGameStarted: (started: boolean) => void;
        setCurrentRoute: (route: string) => void;
    };

export const GameDataContext = createContext<GameData | undefined>(undefined);