import PlayerList from '@/components/PlayerList';
import BasicButton from '../components/BasicButton';
import BasicInput from "../components/BasicInput";
import SubmitButton from '../components/SubmitButton';
import HeadingTwo from "@/components/HeadingTwo";
import { Player } from ".././models/entities/Player";
import { CharacterName } from '@/models/Enums/CharacterName';
import { Archetype } from '@/models/Enums/Archetype';
import { FallSpeed } from '@/models/Enums/FallSpeed';
import { TierPlacement } from '@/models/Enums/TierPlacement';
import { WeightClass } from '@/models/Enums/WeightClass';
import { RequestService } from '@/services/RequestService';


const Lobby = () =>
{
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
            currentCharacter:
            {
                characterName: CharacterName.PYRA_AND_MYTHRA,
                archetype: Archetype.GRAPPLER,
                fallSpeed: FallSpeed.FAST_FALLERS,
                tierPlacement: TierPlacement.C,
                weightClass: WeightClass.SUPER_HEAVYWEIGHT
            },
            currentGameId: sessionCode


        },
        playerTwo:
        {
            id: "randomString2",
            userId: "randomString2",
            displayName: "easton",
            currentScore: 0,
            currentRound: 0,
            currentCharacter:
            {
                characterName: CharacterName.MEGA_MAN,
                archetype: Archetype.GRAPPLER,
                fallSpeed: FallSpeed.FAST_FALLERS,
                tierPlacement: TierPlacement.C,
                weightClass: WeightClass.SUPER_HEAVYWEIGHT
            },
            currentGameId: sessionCode
        },
        playerThree:
        {
            id: "randomString3",
            userId: "randomString3",
            displayName: "koby",
            currentScore: 0,
            currentRound: 0,
            currentCharacter:
            {
                characterName: CharacterName.MARIO,
                archetype: Archetype.GRAPPLER,
                fallSpeed: FallSpeed.FAST_FALLERS,
                tierPlacement: TierPlacement.C,
                weightClass: WeightClass.SUPER_HEAVYWEIGHT
            },
            currentGameId: sessionCode

        }
    }

    const dummyPlayers: Player[] =
        [
            players.playerOne, players.playerTwo, players.playerThree
        ]

    console.log(sessionCode);

    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
                <title>Not Found</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <HeadingTwo headingText="Current Players in Game" />
                    <PlayerList players={dummyPlayers} />
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