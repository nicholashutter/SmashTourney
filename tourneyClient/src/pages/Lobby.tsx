import PlayerList from '@/components/PlayerList';
import BasicButton from '@/components/BasicButton';
import BasicInput from '@/components/BasicInput';
import SubmitButton from '@/components/SubmitButton';
import HeadingTwo from "@/components/HeadingTwo";
import { Player } from ".././models/entities/Player";
import { RequestService } from '@/services/RequestService';
import Mario from "../models/entities/Characters/Mario";
import PersistentConnection from "../services/PersistentConnection"
import { useEffect, useState } from 'react';


const Lobby = () =>
{
    const lobbyConnection = new PersistentConnection();
    const sessionCode = "P1@C3H0!D3R";


    const Players: Player[] =
        [

        ]

    Players.forEach(player => { lobbyConnection.createPlayerConnection() })

    console.log(sessionCode);

    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
                <title>Not Found</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <HeadingTwo headingText="Current Players in Game" />
                    <PlayerList players={Players} />
                    <BasicInput labelText="Session Code:" name="sessionCode" htmlFor="sessionCode" id="sessionCode" value={sessionCode} onChange={() => { }} />
                    <HeadingTwo headingText="Waiting On Additional Players..." />

                    <SubmitButton buttonLabel="All Players In" onSubmit={() =>
                    {
                        RequestService(
                            "startGame",
                            {
                                body:
                                {
                                    //TODO make sure only the first player to join
                                    //can see the all players in button
                                    //accurate request body
                                }
                            }
                        )
                    }
                    } />
                    <BasicButton buttonLabel="Return Home" href="/" />

                </div>
            </div>
        </div>
    );
};

export default Lobby;