import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { AnimatePresence, motion, useReducedMotion } from "framer-motion";
import { useNavigate } from "react-router";
import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton, { SubmitButtonTone } from "@/components/SubmitButton";
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
import { playSound } from "@/services/soundService";

// Length of the in-match redirect countdown, kept in sync with the
// corresponding timer on the bracket page.
const REDIRECT_COUNTDOWN_SECONDS = 15;

// Length of the bracket redirect ring (used to render the SVG progress ring on
// the loading state). The bracket waits 15 s for both players to reach
// the in-match view; the ring shows that wait as a circle draining away.
const COUNTDOWN_RING_RADIUS = 18;
const COUNTDOWN_RING_CIRCUMFERENCE = 2 * Math.PI * COUNTDOWN_RING_RADIUS;

// Renders active-match voting and submits selected winners.
const InMatch = () =>
{
    const navigate = useNavigate();
    const { gameId, playerId } = useGameData();
    const prefersReducedMotion = useReducedMotion();
    const [currentMatch, setCurrentMatch] = useState<CurrentMatchResponse | null>(null);
    const [snapshot, setSnapshot] = useState<BracketSnapshotResponse | null>(null);
    const [gameState, setGameState] = useState<GameStateResponse | null>(null);
    const [gamePlayers, setGamePlayers] = useState<Player[]>([]);
    const [selectedWinnerId, setSelectedWinnerId] = useState<string | null>(null);
    const [status, setStatus] = useState<StatusMessage | null>(null);
    const [isVoteLockedForActiveMatch, setIsVoteLockedForActiveMatch] = useState(false);
    const [opponentHasVoted, setOpponentHasVoted] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [shakeKey, setShakeKey] = useState(0);
    const [secondsUntilMatch, setSecondsUntilMatch] = useState<number | null>(null);
    const activeMatchIdRef = useRef<string | null>(null);
    const activeCountdownMatchIdRef = useRef<string | null>(null);
    const activeMatchDeadlineRef = useRef<number | null>(null);

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

    // The in-match vote can be retried by the user. The shake animation
    // triggers when a previous submit failed, so the user has a visual
    // indication that the previous tap did not stick. The key is bumped each
    // time we want the animation to replay.

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
                setOpponentHasVoted(false);
                setSecondsUntilMatch(null);
                activeCountdownMatchIdRef.current = null;
                activeMatchDeadlineRef.current = null;
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

    // Runs the redirect countdown ring when this player is in the active
    // match but the page has not yet received the "ready" snapshot — e.g. when
    // the bracket view redirected them here too early. The ring is the
    // bracket's 15 s grace period rendered as a draining circle so the wait
    // is legible at a glance.
    const isParticipantInCurrentMatch = Boolean(
        playerId &&
        currentMatch &&
        (currentMatch.playerOneId === playerId || currentMatch.playerTwoId === playerId)
    );

    useEffect(() =>
    {
        if (!isParticipantInCurrentMatch || !canCurrentUserVote)
        {
            setSecondsUntilMatch(null);
            activeCountdownMatchIdRef.current = null;
            activeMatchDeadlineRef.current = null;
            return;
        }

        const matchId = currentMatch?.matchId ?? null;
        if (!matchId)
        {
            return;
        }

        if (activeCountdownMatchIdRef.current !== matchId || !activeMatchDeadlineRef.current)
        {
            activeCountdownMatchIdRef.current = matchId;
            activeMatchDeadlineRef.current = Date.now() + REDIRECT_COUNTDOWN_SECONDS * 1000;
        }

        const updateTimer = () =>
        {
            const deadline = activeMatchDeadlineRef.current ?? Date.now();
            const msRemaining = Math.max(0, deadline - Date.now());
            setSecondsUntilMatch(Math.ceil(msRemaining / 1000));
        };

        updateTimer();
        const intervalId = window.setInterval(updateTimer, 250);

        return () => window.clearInterval(intervalId);
    }, [canCurrentUserVote, currentMatch?.matchId, isParticipantInCurrentMatch]);

    // Submits the selected winner for the current active match.
    const handleLockVote = async () =>
    {
        if (!gameId || !currentMatch || !selectedWinnerId || !canCurrentUserVote)
        {
            return;
        }

        setIsSubmitting(true);

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

            // If the response comes back with `voteCount` greater than one, the
            // other player had already voted before this call landed. The
            // button label flips to "Confirm their call" so the next time the
            // page mounts the player sees the call was theirs to confirm.
            if (voteResult.voteCount >= 2 && voteFeedback.lockVoteForCurrentMatch)
            {
                setOpponentHasVoted(true);
            }

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
                // Play the punch on a committed match — the click is a user
                // gesture, so the autoplay policy allows it.
                if (voteResult.status === "COMMITTED")
                {
                    playSound("voteCommit", { startOffsetSeconds: 0.4 });
                }
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

            // Replay the shake animation on the next render by bumping the
            // key. The wrapper that consumes the key forces a remount of the
            // inner span, which restarts the CSS animation.
            setShakeKey((previous) => previous + 1);
        }
        finally
        {
            setIsSubmitting(false);
        }
    };

    // Resolves the tone and label of the "Lock in Vote" button. The button
    // changes shape to communicate the game state without the user having to
    // read the status banner.
    const lockButtonLabel = (() =>
    {
        if (isVoteLockedForActiveMatch)
        {
            return "Locked \u2713";
        }

        if (opponentHasVoted && !isVoteLockedForActiveMatch)
        {
            return "Confirm their call";
        }

        return "Lock in Vote.";
    })();

    const lockButtonTone: SubmitButtonTone = (() =>
    {
        if (isVoteLockedForActiveMatch)
        {
            return "success";
        }

        if (opponentHasVoted)
        {
            return "success";
        }

        return "default";
    })();

    // The countdown ring only shows when we know there is a wait. After the
    // deadline the ring is full (length 0) and the page moves on, so a
    // negative progress is impossible.
    const ringProgress = secondsUntilMatch == null
        ? 0
        : Math.max(0, Math.min(1, secondsUntilMatch / REDIRECT_COUNTDOWN_SECONDS));
    const ringStrokeDash = COUNTDOWN_RING_CIRCUMFERENCE * (1 - ringProgress);

    return (
        <PageShell pageTitle={`${playerOneName}VS. ${playerTwoName}`}>
                <div className='relative shrink flex flex-col text-2xl p-4 m-4 '>
                    {/* Mirrors the bracket's LIVE indicator with `aria-live` so
                        screen-reader users get the same cue the visual pulse
                        gives sighted users. */}
                    {currentMatch && isParticipantInCurrentMatch && (
                        <motion.div
                            initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, y: -8 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ duration: 0.2 }}
                            className="flex items-center justify-center gap-2 mb-3"
                            role="status"
                            aria-live="polite"
                        >
                            <span
                                aria-hidden="true"
                                className="inline-block w-2 h-2 rounded-full bg-yellow-300"
                            />
                            <span className="text-sm uppercase tracking-wider font-bold text-yellow-300">
                                Match {currentMatch.matchNumber} of Round {currentMatch.round} &middot; Live
                            </span>
                        </motion.div>
                    )}

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
                                                <SubmitButton
                                                    buttonLabel={lockButtonLabel}
                                                    tone={lockButtonTone}
                                                    disabled={isVoteLockedForActiveMatch}
                                                    isLoading={isSubmitting}
                                                    shakeKey={shakeKey}
                                                    onSubmit={handleLockVote}
                                                />
                                            </motion.div>
                                        )}
                                    </AnimatePresence>
                                </>
                            ) : (
                                <HeadingTwo headingText="Match in progress. Waiting for result..." />
                            )}
                        </>
                    ) : (
                        <div className="flex flex-col items-center gap-3 text-white" role="status" aria-live="polite">
                            <div className="flex items-center gap-2" aria-hidden="true">
                                <span className="w-3 h-3 rounded-full bg-yellow-300 animate-pulse [animation-delay:-0.3s]" />
                                <span className="w-3 h-3 rounded-full bg-yellow-300 animate-pulse [animation-delay:-0.15s]" />
                                <span className="w-3 h-3 rounded-full bg-yellow-300 animate-pulse" />
                            </div>
                            <HeadingTwo headingText="Waiting for next match..." />
                            {secondsUntilMatch !== null && (
                                <div className="flex flex-col items-center gap-1">
                                    <svg
                                        width="48"
                                        height="48"
                                        viewBox="0 0 48 48"
                                        className="text-yellow-300"
                                        aria-hidden="true"
                                    >
                                        <circle
                                            cx="24"
                                            cy="24"
                                            r={COUNTDOWN_RING_RADIUS}
                                            stroke="currentColor"
                                            strokeWidth="3"
                                            fill="none"
                                            opacity="0.2"
                                        />
                                        <circle
                                            cx="24"
                                            cy="24"
                                            r={COUNTDOWN_RING_RADIUS}
                                            stroke="currentColor"
                                            strokeWidth="3"
                                            fill="none"
                                            strokeLinecap="round"
                                            strokeDasharray={COUNTDOWN_RING_CIRCUMFERENCE}
                                            strokeDashoffset={ringStrokeDash}
                                            transform="rotate(-90 24 24)"
                                            className="transition-[stroke-dashoffset] duration-300 ease-out"
                                        />
                                    </svg>
                                    <p className="text-sm text-white/80">Match loading in {secondsUntilMatch}s</p>
                                </div>
                            )}
                        </div>
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
