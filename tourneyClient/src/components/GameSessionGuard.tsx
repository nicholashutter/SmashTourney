import { useEffect, useRef } from "react";
import { useLocation, useNavigate } from "react-router";
import { useGameData } from "@/hooks/useGameData";

const GameSessionGuard = () =>
{
    const location = useLocation();
    const navigate = useNavigate();

    const {
        gameId,
        playerId,
        gameStarted,
        currentRoute,
        setCurrentRoute,
    } = useGameData();

    const hasRestoredRoute = useRef(false);

    const shouldLockNavigation = gameStarted && Boolean(gameId) && Boolean(playerId);

    useEffect(() =>
    {
        if (!shouldLockNavigation)
        {
            hasRestoredRoute.current = false;
            return;
        }

        if (!hasRestoredRoute.current && currentRoute && location.pathname !== currentRoute)
        {
            hasRestoredRoute.current = true;
            navigate(currentRoute, { replace: true });
            return;
        }

        hasRestoredRoute.current = true;
    }, [shouldLockNavigation, currentRoute, location.pathname, navigate]);

    useEffect(() =>
    {
        if (!shouldLockNavigation)
        {
            return;
        }

        setCurrentRoute(location.pathname);
    }, [shouldLockNavigation, location.pathname, setCurrentRoute]);

    useEffect(() =>
    {
        if (!shouldLockNavigation)
        {
            return;
        }

        const handlePopState = () =>
        {
            const routeToStayOn = currentRoute || location.pathname;
            window.history.pushState(null, "", routeToStayOn);

            if (location.pathname !== routeToStayOn)
            {
                navigate(routeToStayOn, { replace: true });
            }
        };

        window.history.pushState(null, "", location.pathname);
        window.addEventListener("popstate", handlePopState);

        return () =>
        {
            window.removeEventListener("popstate", handlePopState);
        };
    }, [shouldLockNavigation, currentRoute, location.pathname, navigate]);

    return null;
};

export default GameSessionGuard;
