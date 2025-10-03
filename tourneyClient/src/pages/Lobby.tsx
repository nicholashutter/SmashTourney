import PlayerList from '@/components/PlayerList';
import BasicButton from '@/components/BasicButton';
import BasicInput from '@/components/BasicInput';
import SubmitButton from '@/components/SubmitButton';
import HeadingTwo from "@/components/HeadingTwo";
import { Player } from ".././models/entities/Player";
import { RequestService } from '@/services/RequestService';
import PersistentConnection from "../services/PersistentConnection";
import { useEffect, useState } from 'react';
import { useGameData } from '@/components/GameIdContext';
import { SERVER_ERROR, SUBMIT_SUCCESS } from '@/constants/AppConstants';
import { useNavigate } from 'react-router';

/* 

    Determine who is the first player to join lobby with given gameId
    That player should be the only one who sees "all players in" button
    This is probably handled with a lock object
    
 */

const Lobby = () =>
{
    const [players, setPlayers] = useState<Player[]>([]);

    //open connection to front end signalR hub wrapper
    const lobbyConnection = new PersistentConnection();

    //get the gameId from useContext wrapper
    //should have either been loaded from joinTourney or createTourney pages
    const { Id } = useGameData();

    const navigate = useNavigate();

    useEffect(() =>
    {
        //sync wrapper for useEffect over async call to api
        const asyncRequest = async () =>
        {
            const result: Player[] = await RequestService("getPlayersInGame",
                {
                    body:
                    {
                        gameId: Id,
                    }
                }
            )
            //setPlayers array to result from api
            //this should load players already in game on page load
            setPlayers(result);
        };

        asyncRequest();


        //create connections for all players in game to signalR hub
        lobbyConnection.createPlayerConnection();
        lobbyConnection.setOnPlayersUpdated((updatedPlayers) =>
        {
            setPlayers(updatedPlayers);
        });
        lobbyConnection.setGameId(Id!);
    }, []);

    const handleSubmit = async () =>
    {
        try
        {
            await RequestService("startGame");
            window.alert(SUBMIT_SUCCESS("Join Game"));
            navigate("/inMatch");
        }
        catch (err)
        {
            window.alert(SERVER_ERROR("Start Game"));
            console.log(err);
        }



    };

    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
                <title>Not Found</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <HeadingTwo headingText="Current Players in Game" />
                    <PlayerList players={players} />
                    <BasicInput labelText="Session Code:" name="sessionCode" htmlFor="sessionCode" id="sessionCode" value={Id!} onChange={() => { }} />
                    <HeadingTwo headingText="Waiting On Additional Players..." />

                    <SubmitButton buttonLabel="All Players In" onSubmit={
                        handleSubmit
                    } />
                    <BasicButton buttonLabel="Return Home" href="/" />

                </div>
            </div>
        </div>
    );
};

export default Lobby;