import { useEffect, useRef, useState } from "react";
import { Navigate, Outlet, useParams } from "react-router";
import { useGameData } from "@/hooks/useGameData";
import { resumePlayerSession } from "@/services/playerSessionService";
import PageShell from "@/components/PageShell";
import HeadingTwo from "@/components/HeadingTwo";

// Restores a game session from the game id in the URL before the screen renders.
//
// A player on a phone loses their tab for ordinary reasons — the browser
// reclaims a backgrounded tab, the screen locks long enough, they close it by
// accident. All of that clears sessionStorage, which is where identity used to
// live, so there was no way back into a running tournament.
//
// What survives is the identity cookie and the URL. That is enough: the server
// already stores which player belongs to which user in which game, so this asks
// it rather than trusting anything the client kept. The screens below render
// only once the answer is in, so no page ever has to cope with a half-restored
// session.
const GameRouteGuard = () =>
{
    const { gameId: routeGameId } = useParams();

    const {
        gameId,
        playerId,
        setGameId,
        setPlayerId,
        setIsHost,
        setGameStarted,
    } = useGameData();

    const [resumeStatus, setResumeStatus] = useState<"resolving" | "ready" | "notParticipant" | "failed">("resolving");

    // Tracks which game id has already been resolved so a re-render caused by
    // writing the session back into context does not start the request again.
    const resolvedGameIdRef = useRef<string | null>(null);

    useEffect(() =>
    {
        if (!routeGameId)
        {
            return;
        }

        // The session is already in hand for this game — the normal case when
        // navigating between screens rather than arriving cold.
        if (resolvedGameIdRef.current === routeGameId)
        {
            return;
        }

        if (gameId === routeGameId && playerId)
        {
            resolvedGameIdRef.current = routeGameId;
            setResumeStatus("ready");
            return;
        }

        let isDisposed = false;
        setResumeStatus("resolving");

        const restoreSession = async () =>
        {
            const outcome = await resumePlayerSession(routeGameId);

            if (isDisposed)
            {
                return;
            }

            if (outcome.kind === "notParticipant")
            {
                setResumeStatus("notParticipant");
                return;
            }

            if (outcome.kind === "failed")
            {
                setResumeStatus("failed");
                return;
            }

            setGameId(outcome.session.gameId);
            setPlayerId(outcome.session.playerId);
            setIsHost(outcome.session.isHost);
            setGameStarted(outcome.session.gameStarted);

            resolvedGameIdRef.current = routeGameId;
            setResumeStatus("ready");
        };

        restoreSession();

        return () =>
        {
            isDisposed = true;
        };
    }, [routeGameId, gameId, playerId, setGameId, setPlayerId, setIsHost, setGameStarted]);

    if (!routeGameId)
    {
        return <Navigate to="/tourneyMenu" replace />;
    }

    // Someone opened a link to a game they have not joined. The join screen can
    // prefill the code from the URL, so this is a redirect rather than a dead end.
    if (resumeStatus === "notParticipant")
    {
        return <Navigate to={`/joinTourney?gameId=${encodeURIComponent(routeGameId)}`} replace />;
    }

    if (resumeStatus === "failed")
    {
        return (
            <PageShell pageTitle="Reconnecting">
                <div className="shrink flex flex-col text-2xl p-4 m-4">
                    <HeadingTwo headingText="Could not reach the tournament. Check your connection and reload." />
                </div>
            </PageShell>
        );
    }

    if (resumeStatus === "resolving")
    {
        return (
            <PageShell pageTitle="Reconnecting">
                <div className="shrink flex flex-col text-2xl p-4 m-4">
                    <HeadingTwo headingText="Reconnecting to your tournament..." />
                </div>
            </PageShell>
        );
    }

    return <Outlet />;
};

export default GameRouteGuard;
