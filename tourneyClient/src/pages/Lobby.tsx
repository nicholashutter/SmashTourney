import PlayerList from '@/components/PlayerList';
import BasicButton from '@/components/BasicButton';
import SubmitButton from '@/components/SubmitButton';
import HeadingTwo from "@/components/HeadingTwo";
import { Player } from "@/models/entities/Player";
import { RequestService } from '@/services/RequestService';
import PersistentConnection from "@/services/PersistentConnection";
import { useCallback, useEffect, useRef, useState } from 'react';
import { useGameData } from '@/hooks/useGameData';
import { useNavigate } from 'react-router';
import { normalizePlayers, resolvePlayerId } from '@/lib/normalizePlayer';
import { fetchGameState, fetchPlayersInGame } from '@/services/gameFlowService';


// Renders the lobby and handles realtime player updates before game start.
const Lobby = () =>
{
    const JOIN_NOTICE_DURATION_MS = 2500;

    const resolvePlayerIdCallback = useCallback((player: Player): string =>
    {
        return resolvePlayerId(player);
    }, []);

    const normalizePlayersCallback = useCallback((incomingPlayers: Player[]): Player[] =>
    {
        return normalizePlayers(incomingPlayers);
    }, []);

    const [players, setPlayers] = useState<Player[]>([]);
    const [isLoadingPlayers, setIsLoadingPlayers] = useState(true);
    const [joinNotice, setJoinNotice] = useState<string | null>(null);
    const lobbyConnectionRef = useRef<PersistentConnection | null>(null);

    //get playerId, gameId and setter functions from useContext wrapper
    //should have either been loaded from joinTourney or createTourney pages
    const { gameId, isHost, setGameStarted } = useGameData();

    const navigate = useNavigate();

    useEffect(() =>
    {
        if (!gameId)
        {
            setIsLoadingPlayers(false);
            return;
        }

        const lobbyConnection = new PersistentConnection();
        lobbyConnectionRef.current = lobbyConnection;
        let isDisposed = false;
        let gameStartedPollId: number | null = null;
        let joinNoticeTimeoutId: number | null = null;

        const clearJoinNoticeTimer = () =>
        {
            if (joinNoticeTimeoutId)
            {
                window.clearTimeout(joinNoticeTimeoutId);
                joinNoticeTimeoutId = null;
            }
        };

        const loadLobbyPlayers = async (targetGameId: string) =>
        {
            try
            {
                const playersInGame = await fetchPlayersInGame(targetGameId);
                if (!isDisposed)
                {
                    setPlayers(playersInGame);
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

        const checkStartedState = async (targetGameId: string): Promise<boolean> =>
        {
            try
            {
                const flowState = await fetchGameState(targetGameId);

                if (!isDisposed && flowState && flowState.state !== "LOBBY_WAITING")
                {
                    setGameStarted(true);
                    navigate("/showBracket", { replace: true });
                    return true;
                }
            }
            catch (error)
            {
                console.warn("Lobby start-state check failed", error);
            }

            return false;
        };

        const buildJoinNoticeText = (newPlayers: Player[]): string =>
        {
            const newNames = newPlayers.map((player) => player.displayName || "A player");
            return newNames.length === 1
                ? `${newNames[0]} joined the lobby.`
                : `${newNames.join(", ")} joined the lobby.`;
        };

        const scheduleJoinNoticeClear = () =>
        {
            clearJoinNoticeTimer();
            joinNoticeTimeoutId = window.setTimeout(() =>
            {
                if (!isDisposed)
                {
                    setJoinNotice(null);
                }
            }, JOIN_NOTICE_DURATION_MS);
        };

        const calculateNewPlayers = (previousPlayers: Player[], updatedPlayers: Player[]): Player[] =>
        {
            const previousIds = new Set(previousPlayers.map((player) => resolvePlayerIdCallback(player)));
            return updatedPlayers.filter((player) => !previousIds.has(resolvePlayerIdCallback(player)));
        };

        const handlePlayersUpdated = (updatedPlayers: Player[]) =>
        {
            if (isDisposed)
            {
                return;
            }

            setPlayers((previousPlayers) =>
            {
                const normalizedUpdatedPlayers = normalizePlayersCallback(updatedPlayers);
                const newPlayers = calculateNewPlayers(previousPlayers, normalizedUpdatedPlayers);

                if (newPlayers.length > 0)
                {
                    setJoinNotice(buildJoinNoticeText(newPlayers));
                    scheduleJoinNoticeClear();
                }

                return normalizedUpdatedPlayers;
            });
        };

        const handleGameStarted = (startedGameId: string) =>
        {
            if (isDisposed || startedGameId !== gameId)
            {
                return;
            }

            setGameStarted(true);
            navigate("/showBracket", { replace: true });
        };

        const initializeLobby = async () =>
        {
            lobbyConnection.setOnPlayersUpdated(handlePlayersUpdated);
            lobbyConnection.setOnGameStarted(handleGameStarted);
            await lobbyConnection.createPlayerConnection(gameId);
            await loadLobbyPlayers(gameId);

            const started = await checkStartedState(gameId);
            if (!started)
            {
                gameStartedPollId = window.setInterval(async () =>
                {
                    const hasStarted = await checkStartedState(gameId);
                    if (hasStarted && gameStartedPollId)
                    {
                        window.clearInterval(gameStartedPollId);
                        gameStartedPollId = null;
                    }
                }, 1500);
            }
        };

        initializeLobby();

        return () =>
        {
            isDisposed = true;
            clearJoinNoticeTimer();
            if (gameStartedPollId)
            {
                window.clearInterval(gameStartedPollId);
                gameStartedPollId = null;
            }
            lobbyConnection.disconnect();
            if (lobbyConnectionRef.current === lobbyConnection)
            {
                lobbyConnectionRef.current = null;
            }
        };

    }, [gameId, navigate, normalizePlayersCallback, resolvePlayerIdCallback, setGameStarted]);

    // Starts the game and notifies all connected lobby participants.
    const handleSubmit = async () =>
    {
        try
        {
            if (!gameId)
            {
                window.alert("Start game failed because this lobby does not have a session code. You will stay in the lobby.");
                return;
            }

            await RequestService("startGame", {
                routeParams: { gameId }
            });

            setGameStarted(true);
            if (lobbyConnectionRef.current)
            {
                await lobbyConnectionRef.current.notifyGameStarted(gameId);
            }

            window.alert("All players are in. Starting the tournament now and moving everyone to the bracket view.");
            navigate("/showBracket");
        }
        catch (err)
        {
            window.alert("We could not start the game. You will stay in the lobby so you can try again.");
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
                    {isHost &&
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