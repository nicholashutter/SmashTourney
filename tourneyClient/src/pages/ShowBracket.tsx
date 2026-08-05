
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router";
import DrawWinnersBracket from "@/components/brackets/DynamicBracket";
import WinnerCelebration from "@/components/WinnerCelebration";
import BracketMatchDetailPopover from "@/components/BracketMatchDetailPopover";
import { useGameData } from "@/hooks/useGameData";
import { BracketMatchView, BracketSnapshotResponse, CurrentMatchResponse, GameStateResponse } from "@/models/entities/Bracket";
import { fetchBracketViewData } from "@/services/gameFlowService";
import { inMatchPath, tourneyMenuPath } from "@/services/gameRoutes";
import { playSound } from "@/services/soundService";
import { BasicButton } from "@/components/BasicButton";

// Renders the live tournament bracket and routes players into active matches.
const ShowBracket = () =>
{
    const navigate = useNavigate();
    const { gameId, playerId, setGameStarted } = useGameData();
    const [snapshot, setSnapshot] = useState<BracketSnapshotResponse | null>(null);
    const [currentMatch, setCurrentMatch] = useState<CurrentMatchResponse | null>(null);
    const [gameState, setGameState] = useState<GameStateResponse | null>(null);
    const [secondsUntilMatch, setSecondsUntilMatch] = useState<number | null>(null);
    const [showWinnerCelebration, setShowWinnerCelebration] = useState(false);
    const [championName, setChampionName] = useState<string | null>(null);
    const [focusedMatch, setFocusedMatch] = useState<{ match: BracketMatchView; position: { x: number; y: number } } | null>(null);
    const [bracketContainerRect, setBracketContainerRect] = useState<DOMRect | null>(null);
    const hasPlayedCelebrationRef = useRef(false);
    const activeMatchDeadlineRef = useRef<number | null>(null);
    const activeCountdownMatchIdRef = useRef<string | null>(null);
    const bracketContainerRef = useRef<HTMLDivElement | null>(null);

    const isPlayerInCurrentMatch = Boolean(
        playerId &&
        currentMatch &&
        (currentMatch.playerOneId === playerId || currentMatch.playerTwoId === playerId)
    );

    // Loads and refreshes bracket, current match, and flow-state view data.
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
                const bracketViewData = await fetchBracketViewData(gameId);

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
                console.warn("Bracket refresh failed; the next poll will retry.");
            }
        };

        loadBracketViewData();
        const refreshInterval = window.setInterval(loadBracketViewData, 2000);

        return () =>
        {
            mounted = false;
            window.clearInterval(refreshInterval);
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

            if (msRemaining === 0 && gameId)
            {
                navigate(inMatchPath(gameId), { replace: true });
            }
        };

        updateTimer();
        const countdownInterval = window.setInterval(updateTimer, 250);

        return () =>
        {
            window.clearInterval(countdownInterval);
        };
    }, [currentMatch?.matchId, gameId, gameState?.state, isPlayerInCurrentMatch, navigate, setGameStarted]);

    // Detects the COMPLETE game state and shows the winner celebration. The
    // champion is resolved as the winnerId of the last COMPLETE GRAND_FINALS
    // (or GRAND_FINALS_RESET) match. The `hasPlayedCelebrationRef` ensures
    // the sound and overlay only fire once per tournament — the bracket
    // polls every 2 s, so without the guard the celebration would replay on
    // every refresh.
    useEffect(() =>
    {
        if (!snapshot || gameState?.state !== "COMPLETE")
        {
            return;
        }

        if (hasPlayedCelebrationRef.current)
        {
            return;
        }
        hasPlayedCelebrationRef.current = true;

        const finals = snapshot.matches.filter(
            (match) => match.lane === "GRAND_FINALS" || match.lane === "GRAND_FINALS_RESET"
        );
        const sortedFinals = [...finals].sort((left, right) =>
        {
            if (left.round !== right.round)
            {
                return right.round - left.round;
            }
            return right.matchNumber - left.matchNumber;
        });
        const lastCompleteFinal = sortedFinals.find((match) => match.status === "COMPLETE");
        if (!lastCompleteFinal || !lastCompleteFinal.winnerId)
        {
            return;
        }

        const champion = snapshot.players.find((player) => player.playerId === lastCompleteFinal.winnerId);
        const name = champion?.displayName ?? "the champion";
        setChampionName(name);
        setShowWinnerCelebration(true);
        playSound("tournamentComplete");
    }, [gameState?.state, snapshot]);

    // Listens for the auto-dismiss event from WinnerCelebration.
    useEffect(() =>
    {
        const handler = () =>
        {
            setShowWinnerCelebration(false);
        };
        window.addEventListener("smash-tourney:winner-done", handler);
        return () => window.removeEventListener("smash-tourney:winner-done", handler);
    }, []);

    // Tracks the bracket container's bounding rect. The popover uses this
    // to map SVG viewBox coordinates back to pixel space.
    useEffect(() =>
    {
        if (!bracketContainerRef.current)
        {
            return;
        }

        const updateRect = () =>
        {
            if (bracketContainerRef.current)
            {
                setBracketContainerRect(bracketContainerRef.current.getBoundingClientRect());
            }
        };

        updateRect();

        const resizeObserver = new ResizeObserver(updateRect);
        resizeObserver.observe(bracketContainerRef.current);
        window.addEventListener("scroll", updateRect, { passive: true });
        window.addEventListener("resize", updateRect);

        return () =>
        {
            resizeObserver.disconnect();
            window.removeEventListener("scroll", updateRect);
            window.removeEventListener("resize", updateRect);
        };
    }, []);

    const handleMatchFocus = useCallback((match: BracketMatchView, position: { x: number; y: number }) =>
    {
        setFocusedMatch({ match, position });
    }, []);

    const handleMatchBlur = useCallback(() =>
    {
        setFocusedMatch(null);
    }, []);

    const playersByIdForPopover = useMemo(() =>
    {
        if (!snapshot)
        {
            return new Map();
        }
        return new Map(snapshot.players.map((player) => [player.playerId, player]));
    }, [snapshot]);

    const viewBoxForPopover = useMemo(() =>
    {
        // Recompute the viewBox whenever the rect changes so the popover
        // follows resize events. The dependency is `bracketContainerRect`
        // because the rect itself is the signal the bracket container's size
        // changed.
        if (!bracketContainerRect)
        {
            return null;
        }
        const safeWidth = Math.max(640, bracketContainerRect.width);
        const safeHeight = Math.max(420, bracketContainerRect.height);
        return { width: safeWidth, height: safeHeight };
    }, [bracketContainerRect]);

    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw px-2">
            <div className="relative flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white w-[96vw] h-[92dvh]">
                <title>Current Score</title>
                <div ref={bracketContainerRef} className='relative flex-1 min-h-0 w-full p-4'>
                    <DrawWinnersBracket
                        snapshot={snapshot}
                        currentMatchId={currentMatch?.matchId ?? gameState?.currentMatchId}
                        viewerPlayerId={playerId}
                        onMatchFocus={handleMatchFocus}
                        onMatchBlur={handleMatchBlur}
                    />
                    <BracketMatchDetailPopover
                        match={focusedMatch?.match ?? null}
                        playersById={playersByIdForPopover}
                        containerRect={bracketContainerRect}
                        matchPosition={focusedMatch?.position ?? null}
                        viewBox={viewBoxForPopover}
                        onClose={handleMatchBlur}
                    />
                </div>
                {isPlayerInCurrentMatch && secondsUntilMatch !== null && (
                    <div className="flex items-center justify-center gap-3 pb-4 text-white">
                        <svg
                            width="36"
                            height="36"
                            viewBox="0 0 36 36"
                            className="text-yellow-300"
                            aria-hidden="true"
                        >
                            <circle
                                cx="18"
                                cy="18"
                                r="14"
                                stroke="currentColor"
                                strokeWidth="3"
                                fill="none"
                                opacity="0.2"
                            />
                            <circle
                                cx="18"
                                cy="18"
                                r="14"
                                stroke="currentColor"
                                strokeWidth="3"
                                fill="none"
                                strokeLinecap="round"
                                strokeDasharray={2 * Math.PI * 14}
                                strokeDashoffset={2 * Math.PI * 14 * Math.max(0, Math.min(1, secondsUntilMatch / 15))}
                                transform="rotate(-90 18 18)"
                                className="transition-[stroke-dashoffset] duration-300 ease-out"
                            />
                        </svg>
                        <p className="text-sm">Your match is loading in {secondsUntilMatch}s...</p>
                    </div>
                )}
                {!isPlayerInCurrentMatch && (
                    <p className="text-sm text-white pb-4">Waiting for your next match...</p>
                )}
                <WinnerCelebration
                    show={showWinnerCelebration && championName !== null}
                    championName={championName ?? ""}
                    footer={
                        <BasicButton
                            buttonLabel="Back to menu"
                            href={tourneyMenuPath()}
                        />
                    }
                />
            </div>
        </div>

    );
}
export { ShowBracket };