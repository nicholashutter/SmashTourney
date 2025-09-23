import React, {createContext, useState, useContext, ReactNode} from "react";


//setup type that holds Id itself and callback function for useState
type GameId = 
{
    Id: string|null;
    setId: (id: string) => void;
}

//create the context so that gameId can be accessed globally
const GameIdContext = createContext<GameId|undefined>(undefined); 


//define and export the provider component for the gameIdContext object
export const GameIdDataProvider = ({children}: {children: ReactNode}) => 
    {
        const [Id, setId] = useState<string|null>(null); 

        return (
            <GameIdContext.Provider value={{Id, setId}}> 
            {children}
            </GameIdContext.Provider>
        );
    };

//define and export the custom hook to access the gameId locally
export const useGameId = () =>
{
    const context = useContext(GameIdContext);
    if(!context)
    {
        throw new Error("this Hook must be used within useGameIdProvider"); 
    }
    return context; 
}