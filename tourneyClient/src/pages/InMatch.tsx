import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router";
import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton from "@/components/SubmitButton";
import { useGameData } from "@/hooks/useGameData";
import { RequestService } from "@/services/RequestService";
import
{
    BracketSnapshotResponse,
    CurrentMatchResponse,
    GameStateResponse,
    SubmitMatchVoteRequest,
    SubmitMatchVoteResponse
} from "@/models/entities/Bracket";
import { Player } from "@/models/entities/Player";
import { resolvePlayerId } from "@/lib/normalizePlayer";
import { fetchInMatchViewData } from "@/services/gameFlowService";
import { getVoteFeedbackFromError, getVoteFeedbackFromResponse } from "@/services/matchVoteFeedback";
import { resolveInMatchRedirect } from "@/services/inMatchRouting";

// Renders active-match voting and submits selected winners.
const InMatch = () =>
{
    const navigate = useNavigate();
    const { gameId, playerId } = useGameData();
    const [currentMatch, setCurrentMatch] = useState<CurrentMatchResponse | null>(null);
    const [snapshot, setSnapshot] = useState<BracketSnapshotResponse | null>(null);
    const [gameState, setGameState] = useState<GameStateResponse | null>(null);
    const [gamePlayers, setGamePlayers] = useState<Player[]>([]);
    const [selectedWinnerId, setSelectedWinnerId] = useState<string | null>(null);
    const [voteNotice, setVoteNotice] = useState<string | null>(null);
    const [isVoteLockedForActiveMatch, setIsVoteLockedForActiveMatch] = useState(false);
    const activeMatchIdRef = useRef<string | null>(null);

    // Builds a lookup table from player identifiers to display names.
    const playersById = useMemo(() =>
    {
        const entries = new Map<string, string>();

        (gamePlayers ?? []).forEach((player) =>
        {
            const resolvedPlayerId = resolvePlayerId(player);
            if (resolvedPlayerId)
            {
                entries.set(resolvedPlayerId, player.displayName);
            }
        });

        if (snapshot)
        {
            for (const player of snapshot.players)
            {
                if (!entries.has(player.playerId))
                {
                    entries.set(player.playerId, player.displayName);
                }
            }
        }

        return entries;
    }, [gamePlayers, snapshot]);

    let playerOneName = "Player One";
    if (currentMatch && currentMatch.playerOneId)
    {
        const resolvedName = playersById.get(currentMatch.playerOneId);
        if (resolvedName)
        {
            playerOneName = resolvedName;
        }
    }

    let playerTwoName = "Player Two";
    if (currentMatch && currentMatch.playerTwoId)
    {
        const resolvedName = playersById.get(currentMatch.playerTwoId);
        if (resolvedName)
        {
            playerTwoName = resolvedName;
        }
    }

    const canCurrentUserVote = Boolean(
        playerId &&
        currentMatch &&
        (currentMatch.playerOneId === playerId || currentMatch.playerTwoId === playerId) &&
        !isVoteLockedForActiveMatch
    );

    // Loads current in-match data from bracket and player endpoints.
    const loadMatchData = useCallback(async () =>
    {
        if (!gameId)
        {
            return;
        }

        try
        {
            const inMatchViewData = await fetchInMatchViewData(gameId);

            const nextMatchId = inMatchViewData.currentMatch?.matchId ?? null;
            if (nextMatchId !== activeMatchIdRef.current)
            {
                activeMatchIdRef.current = nextMatchId;
                setSelectedWinnerId(null);
                setVoteNotice(null);
                setIsVoteLockedForActiveMatch(false);
            }

            setCurrentMatch(inMatchViewData.currentMatch);
            setSnapshot(inMatchViewData.snapshot);
            setGameState(inMatchViewData.gameState);
            setGamePlayers(inMatchViewData.gamePlayers);
        }
        catch (error)
        {
            console.error("Failed to load in-match data", error);
        }
    }, [gameId]);

    // Loads initial in-match state when the page opens.
    useEffect(() =>
    {
        loadMatchData();
    }, [loadMatchData]);

    // Polls while waiting for a votable match (no match yet, or current user is not a participant).
    useEffect(() =>
    {
        if (!gameId)
        {
            return;
        }

        if (currentMatch && canCurrentUserVote)
        {
            return;
        }

        const intervalId = window.setInterval(() =>
        {
            loadMatchData();
        }, 1500);

        return () =>
        {
            window.clearInterval(intervalId);
        };
    }, [canCurrentUserVote, currentMatch, gameId, loadMatchData]);

    // Redirects to the appropriate screen when backend game state indicates in-match is no longer valid.
    useEffect(() =>
    {
        const redirectPath = resolveInMatchRedirect(gameState?.state ?? null, currentMatch, playerId);
        if (redirectPath)
        {
            navigate(redirectPath, { replace: true });
        }
    }, [currentMatch, gameState?.state, navigate, playerId]);

    // Submits the selected winner for the current active match.
    const handleLockVote = async () =>
    {
        if (!gameId || !currentMatch || !selectedWinnerId || !canCurrentUserVote)
        {
            return;
        }

        try
        {
            const payload: SubmitMatchVoteRequest = {
                matchId: currentMatch.matchId,
                winnerPlayerId: selectedWinnerId
            };

            const voteResult = await RequestService<"submitMatchVote", SubmitMatchVoteRequest, SubmitMatchVoteResponse>("submitMatchVote", {
                body: payload,
                routeParams: { gameId }
            });

            const voteFeedback = getVoteFeedbackFromResponse(voteResult);

            if (voteFeedback.clearSelectedWinner)
            {
                setSelectedWinnerId(null);
            }

            if (voteFeedback.lockVoteForCurrentMatch)
            {
                setIsVoteLockedForActiveMatch(true);
            }

            if (voteFeedback.noticeMessage)
            {
                setVoteNotice(voteFeedback.noticeMessage);
            }

            if (voteFeedback.alertMessage)
            {
                window.alert(voteFeedback.alertMessage);
            }

            if (voteFeedback.refreshMatchData)
            {
                await loadMatchData();
            }
        }
        catch (error)
        {
            console.error("Failed to submit match vote", error);

            const message = error instanceof Error ? error.message : "";
            const voteFeedback = getVoteFeedbackFromError(message);

            if (voteFeedback.clearSelectedWinner)
            {
                setSelectedWinnerId(null);
            }

            if (voteFeedback.lockVoteForCurrentMatch)
            {
                setIsVoteLockedForActiveMatch(true);
            }

            if (voteFeedback.noticeMessage)
            {
                setVoteNotice(voteFeedback.noticeMessage);
            }

            if (voteFeedback.alertMessage)
            {
                window.alert(voteFeedback.alertMessage);
            }

            if (voteFeedback.refreshMatchData)
            {
                await loadMatchData();
            }
        }
    };

    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
                <title>{playerOneName}VS. {playerTwoName}</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText={`${playerOneName} VS. ${playerTwoName}`} headingColors="white" />
                    {currentMatch ? (
                        <>
                            {canCurrentUserVote ? (
                                <>
                                    <HeadingTwo headingText="Who Won?" />
                                    <SubmitButton buttonLabel={`${playerOneName}${selectedWinnerId === currentMatch.playerOneId ? " ✓" : ""}`} onSubmit={() =>
                                    {
                                        setSelectedWinnerId(currentMatch.playerOneId);
                                    }
                                    } />
                                    <SubmitButton buttonLabel={`${playerTwoName}${selectedWinnerId === currentMatch.playerTwoId ? " ✓" : ""}`} onSubmit={() =>
                                    {
                                        setSelectedWinnerId(currentMatch.playerTwoId);
                                    }
                                    } />
                                    {selectedWinnerId && (
                                        <SubmitButton buttonLabel="Lock in Vote." onSubmit={handleLockVote} />
                                    )}
                                </>
                            ) : (
                                <HeadingTwo headingText="Match in progress. Waiting for result..." />
                            )}
                        </>
                    ) : (
                        <HeadingTwo headingText="Waiting for next match..." />
                    )}
                    {!currentMatch && (
                        <SubmitButton buttonLabel="Refresh Match" onSubmit={loadMatchData} />
                    )}
                    {voteNotice && (
                        <HeadingTwo headingText={voteNotice} />
                    )}
                </div>
            </div>
        </div>

    );
}
export { InMatch };