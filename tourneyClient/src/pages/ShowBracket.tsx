
import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router";
import DrawWinnersBracket from "@/components/brackets/DynamicBracket";
import { useGameData } from "@/hooks/useGameData";
import { areSameId } from "@/lib/idEquality";
import { BracketSnapshotResponse, CurrentMatchResponse, GameStateResponse } from "@/models/entities/Bracket";
import
    {
        connectToGameRealtime,
        fetchRealtimeBracketViewData,
        onBracketUpdatedRealtime,
        onCurrentMatchUpdatedRealtime,
        onFlowStateUpdatedRealtime
    } from "@/services/RealtimeGameService";

// Renders the live tournament bracket and routes players into active matches.
const ShowBracket = () =>
{
    const navigate = useNavigate();
    const { gameId, playerId, setGameStarted } = useGameData();
    const [snapshot, setSnapshot] = useState<BracketSnapshotResponse | null>(null);
    const [currentMatch, setCurrentMatch] = useState<CurrentMatchResponse | null>(null);
    const [gameState, setGameState] = useState<GameStateResponse | null>(null);
    const [secondsUntilMatch, setSecondsUntilMatch] = useState<number | null>(null);
    const activeMatchDeadlineRef = useRef<number | null>(null);
    const activeCountdownMatchIdRef = useRef<string | null>(null);
    const latestLoadRequestRef = useRef(0);
    const reconcileQueuedRef = useRef(false);

    const isPlayerInCurrentMatch = Boolean(
        playerId &&
        currentMatch &&
        (areSameId(currentMatch.playerOneId, playerId) || areSameId(currentMatch.playerTwoId, playerId))
    );

    // Builds ordered participant labels for first-round bracket rendering.
    const bracketPlayerNames = useMemo(() =>
    {
        if (!snapshot)
        {
            return [] as string[];
        }

        const orderedPlayers = [...snapshot.players].sort((left, right) => left.seed - right.seed);
        return orderedPlayers.map((player) => player.displayName);
    }, [snapshot]);

    // Loads and subscribes to bracket, current match, and flow-state view data.
    useEffect(() =>
    {
        let mounted = true;

        const loadBracketViewData = async () =>
        {
            if (!gameId)
            {
                return;
            }

            try
            {
                const requestId = latestLoadRequestRef.current + 1;
                latestLoadRequestRef.current = requestId;
                const bracketViewData = await fetchRealtimeBracketViewData(gameId);

                if (requestId !== latestLoadRequestRef.current)
                {
                    return;
                }

                if (mounted)
                {
                    setSnapshot(bracketViewData.snapshot);
                    setCurrentMatch(bracketViewData.currentMatch);
                    setGameState(bracketViewData.gameState);
                }
            }
            catch (error)
            {
                console.error("Failed to load bracket snapshot", error);
                window.alert("We could not refresh the bracket right now. You will stay on this screen and it will try again.");
            }
        };

        const scheduleRealtimeReconciliation = () =>
        {
            if (reconcileQueuedRef.current)
            {
                return;
            }

            reconcileQueuedRef.current = true;
            window.setTimeout(() =>
            {
                reconcileQueuedRef.current = false;
                if (mounted)
                {
                    void loadBracketViewData();
                }
            }, 0);
        };

        const registerRealtimeHandlers = async () =>
        {
            await connectToGameRealtime(gameId);

            onBracketUpdatedRealtime((nextSnapshot) =>
            {
                if (mounted)
                {
                    setSnapshot(nextSnapshot);
                }

                scheduleRealtimeReconciliation();
            });

            onCurrentMatchUpdatedRealtime((nextCurrentMatch) =>
            {
                if (mounted)
                {
                    setCurrentMatch(nextCurrentMatch);
                }

                scheduleRealtimeReconciliation();
            });

            onFlowStateUpdatedRealtime((nextFlowState) =>
            {
                if (mounted)
                {
                    setGameState(nextFlowState);
                }

                scheduleRealtimeReconciliation();
            });

            await loadBracketViewData();
        };

        registerRealtimeHandlers();

        return () =>
        {
            mounted = false;
            onBracketUpdatedRealtime(() =>
            {
            });
            onCurrentMatchUpdatedRealtime(() =>
            {
            });
            onFlowStateUpdatedRealtime(() =>
            {
            });
        };
    }, [gameId]);

    // Starts a player-facing match countdown and redirects active participants to in-match view.
    useEffect(() =>
    {
        const activeMatchId = currentMatch?.matchId ?? null;
        const isInMatchActive = gameState?.state === "IN_MATCH_ACTIVE";

        if (!isPlayerInCurrentMatch || !isInMatchActive || !activeMatchId)
        {
            setSecondsUntilMatch(null);
            activeMatchDeadlineRef.current = null;
            activeCountdownMatchIdRef.current = null;
            return;
        }

        setGameStarted(true);
        const matchDelayMs = 15000;
        if (activeCountdownMatchIdRef.current !== activeMatchId || !activeMatchDeadlineRef.current)
        {
            activeCountdownMatchIdRef.current = activeMatchId;
            activeMatchDeadlineRef.current = Date.now() + matchDelayMs;
        }

        const updateTimer = () =>
        {
            const deadline = activeMatchDeadlineRef.current ?? Date.now();
            const msRemaining = Math.max(0, deadline - Date.now());
            setSecondsUntilMatch(Math.ceil(msRemaining / 1000));

            if (msRemaining === 0)
            {
                navigate("/inMatch", { replace: true });
            }
        };

        updateTimer();
        const countdownInterval = window.setInterval(updateTimer, 250);

        return () =>
        {
            window.clearInterval(countdownInterval);
        };
    }, [currentMatch?.matchId, gameState?.state, isPlayerInCurrentMatch, navigate, setGameStarted]);

    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw px-2">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white w-[96vw] h-[92dvh]">
                <title>Current Score</title>
                <div className='flex-1 min-h-0 w-full p-4'>
                    <DrawWinnersBracket
                        numPlayers={snapshot ? snapshot.players.length : 16}
                        playerNames={bracketPlayerNames}
                        mode={snapshot?.mode}
                    />
                </div>
                {isPlayerInCurrentMatch && secondsUntilMatch !== null && (
                    <p className="text-sm text-white pb-4">Your match is loading in {secondsUntilMatch}s...</p>
                )}
                {!isPlayerInCurrentMatch && (
                    <p className="text-sm text-white pb-4">Waiting for your next match...</p>
                )}
            </div>
        </div>

    );
}
export { ShowBracket };