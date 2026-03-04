import { createContext } from "react";

// Defines shared tournament session state stored in React context.
export type GameData =
    {
        gameId: string | null;
        playerId: string | null;
        isHost: boolean;
        gameStarted: boolean;
        currentRoute: string | null;
        setGameId: (id: string | null) => void;
        setPlayerId: (id: string | null) => void;
        setIsHost: (host: boolean) => void;
        setGameStarted: (started: boolean) => void;
        setCurrentRoute: (route: string | null) => void;
    };

// Provides a shared context for game session state.
export const GameDataContext = createContext<GameData | undefined>(undefined);