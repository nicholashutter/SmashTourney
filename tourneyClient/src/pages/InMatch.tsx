import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { useNavigate } from "react-router";
import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton from "@/components/SubmitButton";
import PageShell from "@/components/PageShell";
import StatusBanner, { StatusMessage } from "@/components/StatusBanner";
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
    const [status, setStatus] = useState<StatusMessage | null>(null);
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
                setStatus(null);
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
        if (!gameId)
        {
            return;
        }

        const redirectPath = resolveInMatchRedirect(gameState?.state ?? null, currentMatch, playerId, gameId);
        if (redirectPath)
        {
            navigate(redirectPath, { replace: true });
        }
    }, [currentMatch, gameId, gameState?.state, navigate, playerId]);

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
                setStatus({ text: voteFeedback.noticeMessage, tone: "info" });
            }

            // A critical outcome is shown in the same place as a routine one,
            // just in the error tone. Blocking a participant behind a dialog
            // stalled the match for the other player too.
            if (voteFeedback.alertMessage)
            {
                setStatus({ text: voteFeedback.alertMessage, tone: "error" });
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
                setStatus({ text: voteFeedback.noticeMessage, tone: "info" });
            }

            // A critical outcome is shown in the same place as a routine one,
            // just in the error tone. Blocking a participant behind a dialog
            // stalled the match for the other player too.
            if (voteFeedback.alertMessage)
            {
                setStatus({ text: voteFeedback.alertMessage, tone: "error" });
            }

            if (voteFeedback.refreshMatchData)
            {
                await loadMatchData();
            }
        }
    };

    return (

        <PageShell pageTitle={`${playerOneName}VS. ${playerTwoName}`}>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText={`${playerOneName} VS. ${playerTwoName}`} headingColors="white" />
                    {currentMatch ? (
                        <>
                            {canCurrentUserVote ? (
                                <>
                                    <HeadingTwo headingText="Who Won?" />

                                    {/* The chosen winner is scaled and ringed rather than
                                        marked with a tick appended to its label. Both
                                        players are tapping this on their own phone under
                                        time pressure, so which one is selected has to be
                                        obvious at a glance, not read. */}
                                    <motion.div
                                        animate={{ scale: selectedWinnerId === currentMatch.playerOneId ? 1.04 : 1 }}
                                        transition={{ type: "spring", stiffness: 340, damping: 24 }}
                                        className={`rounded ${selectedWinnerId === currentMatch.playerOneId ? "ring-4 ring-yellow-400" : ""}`}
                                    >
                                        <SubmitButton buttonLabel={playerOneName} onSubmit={() =>
                                        {
                                            setSelectedWinnerId(currentMatch.playerOneId);
                                        }
                                        } />
                                    </motion.div>

                                    <motion.div
                                        animate={{ scale: selectedWinnerId === currentMatch.playerTwoId ? 1.04 : 1 }}
                                        transition={{ type: "spring", stiffness: 340, damping: 24 }}
                                        className={`rounded ${selectedWinnerId === currentMatch.playerTwoId ? "ring-4 ring-yellow-400" : ""}`}
                                    >
                                        <SubmitButton buttonLabel={playerTwoName} onSubmit={() =>
                                        {
                                            setSelectedWinnerId(currentMatch.playerTwoId);
                                        }
                                        } />
                                    </motion.div>

                                    <AnimatePresence>
                                        {selectedWinnerId && (
                                            <motion.div
                                                initial={{ opacity: 0, height: 0 }}
                                                animate={{ opacity: 1, height: "auto" }}
                                                exit={{ opacity: 0, height: 0 }}
                                                transition={{ duration: 0.2 }}
                                            >
                                                <SubmitButton buttonLabel="Lock in Vote." onSubmit={handleLockVote} />
                                            </motion.div>
                                        )}
                                    </AnimatePresence>
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
                    <StatusBanner status={status} />
                </div>
        </PageShell>

    );
}
export { InMatch };