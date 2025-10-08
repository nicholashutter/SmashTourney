
import { Player } from "../models/entities/Player";
import { useGameData } from "@/components/GameIdContext";
import { useState, useEffect } from "react";
import { RequestService } from "@/services/RequestService";
import DynamicBracket from "@/components/brackets/DynamicBracket";


const ShowBracket = () =>
{

    const [playersInGame, setPlayersInGame] = useState<Player[]>([]);

    const { gameId } = useGameData();

    useEffect(() =>
    {
        const asyncRequest = async () =>
        {
            setPlayersInGame(await RequestService(
                "getPlayersInGame",
                {
                    body:
                    {
                    },
                    routeParams:
                    {
                        gameId: gameId!,
                    }
                }
            ));
        }
        try
        {
            asyncRequest();
        }
        catch (err)
        {
            console.log(err);
        }
    });


    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>Current Score</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <DynamicBracket numPlayers={16}/>
                </div>
            </div>
        </div>

    );
}
export default ShowBracket;