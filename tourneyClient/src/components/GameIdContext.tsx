import React, { createContext, useState, useContext, ReactNode } from "react";


//setup type that holds front end game stsate including
//  Id and callback function for useState
type GameData =
    {
        Id: string | null;
        setId: (id: string) => void;
    }

//create the context so that gameData can be accessed globally
const GameDataContext = createContext<GameData | undefined>(undefined);


//define and export the provider component for the gameDataContext object
export const GameDataProvider = ({ children }: { children: ReactNode }) => 
{
    const [Id, setId] = useState<string | null>(null);

    return (
        <GameDataContext.Provider value={{ Id, setId }}>
            {children}
        </GameDataContext.Provider>
    );
};

//define and export the custom hook to access the gameData locally
export const useGameData = () =>
{
    const context = useContext(GameDataContext);
    if (!context)
    {
        throw new Error("this Hook must be used within useGameIdProvider");
    }
    return context;
}