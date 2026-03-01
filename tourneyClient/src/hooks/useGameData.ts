import { useContext } from "react";
import { GameDataContext } from "@/components/GameDataContext";

export const useGameData = () =>
{
    const context = useContext(GameDataContext);
    if (!context)
    {
        throw new Error("this Hook must be used within useGameIdProvider");
    }
    return context;
};
