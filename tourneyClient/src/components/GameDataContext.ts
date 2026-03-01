import { createContext } from "react";

export type GameData =
    {
        gameId: string | null;
        playerId: string | null;
        setGameId: (id: string) => void;
        setPlayerId: (id: string) => void;
    };

export const GameDataContext = createContext<GameData | undefined>(undefined);