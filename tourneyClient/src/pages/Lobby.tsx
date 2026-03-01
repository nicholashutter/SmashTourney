import PlayerList from '@/components/PlayerList';
import BasicButton from '@/components/BasicButton';
import SubmitButton from '@/components/SubmitButton';
import HeadingTwo from "@/components/HeadingTwo";
import { Player } from "@/models/entities/Player";
import { RequestService } from '@/services/RequestService';
import PersistentConnection from "@/services/PersistentConnection";
import { useEffect, useState } from 'react';
import { useGameData } from '@/hooks/useGameData';
import { SERVER_ERROR, SUBMIT_SUCCESS } from '@/constants/AppConstants';
import { useNavigate } from 'react-router';



const Lobby = () =>
{
    type GetPlayersInGameResponse = {
        currentPlayers?: Player[];
    };

    const resolvePlayerId = (player: Player): string =>
    {
        return player.Id ?? player.id ?? "";
    };

    const normalizePlayers = (incomingPlayers: Player[]): Player[] =>
    {
        return incomingPlayers.map((player) => ({
            ...player,
            Id: resolvePlayerId(player),
            currentGameId: player.currentGameId ?? player.currentGameID ?? ""
        }));
    };

    const [players, setPlayers] = useState<Player[]>([]);
    const [isLoadingPlayers, setIsLoadingPlayers] = useState(true);
    const [joinNotice, setJoinNotice] = useState<string | null>(null);

    //get playerId, gameId and setter functions from useContext wrapper
    //should have either been loaded from joinTourney or createTourney pages
    const { gameId: gameId, playerId: playerId } = useGameData();


    //explicitly typing as boolean because this type of conditional check confuses me 
    const firstInLobby: boolean = players.length > 0 && resolvePlayerId(players[0]) === playerId;

    const navigate = useNavigate();

    useEffect(() =>
    {
        if (!gameId)
        {
            setIsLoadingPlayers(false);
            return;
        }

        const lobbyConnection = new PersistentConnection();
        let isDisposed = false;

        //sync wrapper for useEffect over async call to api

        //variable shadowing here which should satisfy typescript string|null type error
        const asyncRequest = async (gameId: string) =>
        {
            try
            {
                const result = await RequestService<"getPlayersInGame", Record<string, never>, GetPlayersInGameResponse>("getPlayersInGame",
                    {
                        body:
                        {
                        },
                        routeParams:
                        {
                            gameId
                        }
                    }
                )
                //setPlayers array to result from api
                //this should load players already in game on page load
                if (!isDisposed)
                {
                    setPlayers(normalizePlayers(result.currentPlayers ?? []));
                }
            }
            catch (err)
            {
                console.log(err);
                if (!isDisposed)
                {
                    setPlayers([]);
                }
            }
            finally
            {
                if (!isDisposed)
                {
                    setIsLoadingPlayers(false);
                }
            }
        };

        const initializeLobby = async () =>
        {
            lobbyConnection.setOnPlayersUpdated((updatedPlayers) =>
            {
                if (!isDisposed)
                {
                    setPlayers((previousPlayers) =>
                    {
                        const normalizedUpdatedPlayers = normalizePlayers(updatedPlayers);
                        const previousIds = new Set(previousPlayers.map((player) => resolvePlayerId(player)));
                        const newPlayers = normalizedUpdatedPlayers.filter((player) => !previousIds.has(resolvePlayerId(player)));

                        if (newPlayers.length > 0)
                        {
                            const newNames = newPlayers.map((player) => player.displayName || "A player");
                            const noticeText = newNames.length === 1
                                ? `${newNames[0]} joined the lobby.`
                                : `${newNames.join(", ")} joined the lobby.`;

                            setJoinNotice(noticeText);
                            setTimeout(() =>
                            {
                                if (!isDisposed)
                                {
                                    setJoinNotice(null);
                                }
                            }, 2500);
                        }

                        return normalizedUpdatedPlayers;
                    });
                }
            });
            await lobbyConnection.createPlayerConnection(gameId);
            await asyncRequest(gameId);
        };

        initializeLobby();

        return () =>
        {
            isDisposed = true;
            lobbyConnection.disconnect();
        };

    }, [gameId]);

    const handleSubmit = async () =>
    {
        try
        {
            if (!gameId)
            {
                window.alert(SERVER_ERROR("Start Game"));
                return;
            }

            await RequestService("startGame", {
                routeParams: { gameId }
            });
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
                    <HeadingTwo headingText={`Lobby Players (${players.length})`} />
                    {joinNotice && (
                        <p className="text-base text-white bg-black/40 rounded px-3 py-2 m-2">{joinNotice}</p>
                    )}
                    {isLoadingPlayers ? (
                        <HeadingTwo headingText="Loading players..." />
                    ) : (
                        <PlayerList players={players} />
                    )}
                    <div className="flex flex-col items-center">
                        <label className="text-3xl text-white font-[Arial] text-shadow-lg">Session Code:</label>
                        <p className="bg-white m-5 px-3 py-2 rounded shadow-md text-black text-center max-w-sm w-full break-all text-base">
                            {gameId ?? ""}
                        </p>
                    </div>
                    <HeadingTwo headingText="Waiting for players to join..." />
                    {firstInLobby &&
                        <SubmitButton buttonLabel="All Players In" onSubmit={
                            handleSubmit
                        } />
                    }
                    <BasicButton buttonLabel="Return Home" href="/" />

                </div>
            </div>
        </div>
    );
};

export default Lobby;