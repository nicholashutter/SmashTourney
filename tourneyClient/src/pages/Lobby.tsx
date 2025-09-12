import PlayerList from '@/components/PlayerList';
import BasicButton from '@/components/BasicButton';
import BasicInput from '@/components/BasicInput';
import SubmitButton from '@/components/SubmitButton';
import HeadingTwo from "@/components/HeadingTwo";
import { Player } from ".././models/entities/Player";
import { RequestService } from '@/services/RequestService';
import Mario from "../models/entities/Characters/Mario";
import PersistentConnection from "../services/PersistentConnection"


const Lobby = () =>
{
    const lobbyConnection = new PersistentConnection();
    const sessionCode = "P1@C3H0!D3R";

    const players =
    {
        playerOne:
        {
            id: "randomString1",
            userId: "randomString1",
            displayName: "nicholas",
            currentScore: 0,
            currentRound: 0,
            currentCharacter: Mario,
            currentGameId: sessionCode


        },
        playerTwo:
        {
            id: "randomString2",
            userId: "randomString2",
            displayName: "easton",
            currentScore: 0,
            currentRound: 0,
            currentCharacter: Mario,
            currentGameId: sessionCode
        },
        playerThree:
        {
            id: "randomString3",
            userId: "randomString3",
            displayName: "koby",
            currentScore: 0,
            currentRound: 0,
            currentCharacter: Mario,
            currentGameId: sessionCode

        }
    }
    //this list will be fetched from the rest api
    // once the players are added on the other screen, and the connections are created after the initial fetch
    // the front end should listen for new connections by connecting the signalR hub to the internal gameservice
    const Players: Player[] =
        [
            players.playerOne, players.playerTwo, players.playerThree
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