import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton from "@/components/SubmitButton";
import { useGameData } from "@/hooks/useGameData";
import { RequestService } from "@/services/RequestService";
import
    {
        BracketSnapshotResponse,
        CurrentMatchResponse,
        SubmitMatchVoteRequest,
        SubmitMatchVoteResponse
    } from "@/models/entities/Bracket";
import { Player } from "@/models/entities/Player";
import { resolvePlayerId } from "@/lib/normalizePlayer";
import { fetchInMatchViewData } from "@/services/gameFlowService";
import { getVoteFeedbackFromError, getVoteFeedbackFromResponse } from "@/services/matchVoteFeedback";

// Renders active-match voting and submits selected winners.
const InMatch = () =>
{
    const { gameId, playerId } = useGameData();
    const [currentMatch, setCurrentMatch] = useState<CurrentMatchResponse | null>(null);
    const [snapshot, setSnapshot] = useState<BracketSnapshotResponse | null>(null);
    const [gamePlayers, setGamePlayers] = useState<Player[]>([]);
    const [selectedWinnerId, setSelectedWinnerId] = useState<string | null>(null);
    const [voteNotice, setVoteNotice] = useState<string | null>(null);
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
        (currentMatch.playerOneId === playerId || currentMatch.playerTwoId === playerId)
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
            }

            setCurrentMatch(inMatchViewData.currentMatch);
            setSnapshot(inMatchViewData.snapshot);
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

    // Polls for the next active match while no current match is assigned.
    useEffect(() =>
    {
        if (!gameId || currentMatch)
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
    }, [currentMatch, gameId, loadMatchData]);

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

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
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
export default InMatch;