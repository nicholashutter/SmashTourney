import { useContext } from "react";
import { GameDataContext } from "@/components/GameDataContext";

// Returns shared game session state from the GameData context.
export const useGameData = () =>
{
    const context = useContext(GameDataContext);
    if (!context)
    {
        throw new Error("useGameData must be used within GameDataProvider");
    }
    return context;
};
