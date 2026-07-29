import { useEffect, useState } from "react";
import { Navigate, Outlet, useLocation } from "react-router";
import { RequestService } from "@/services/RequestService";
import { signInPathReturningTo } from "@/services/returnPath";

type AuthState = "loading" | "authenticated" | "unauthenticated";

// Guards protected routes by verifying an active backend session.
const RequireAuth = () =>
{
    const [authState, setAuthState] = useState<AuthState>("loading");
    const location = useLocation();

    useEffect(() =>
    {
        let mounted = true;

        const checkSession = async () =>
        {
            try
            {
                await RequestService<"sessionStatus", never, { IsAuthenticated: boolean }>("sessionStatus");

                if (mounted)
                {
                    setAuthState("authenticated");
                }
            }
            catch
            {
                if (mounted)
                {
                    setAuthState("unauthenticated");
                }
            }
        };

        checkSession();

        return () =>
        {
            mounted = false;
        };
    }, []);

    if (authState === "loading")
    {
        return <div className="text-white text-center p-6">Checking authentication...</div>;
    }

    // The attempted destination travels with the redirect. A player clicking
    // their tournament link on a phone whose session has lapsed should sign in
    // and land back in the game, not at the menu wondering where it went.
    if (authState === "unauthenticated")
    {
        return <Navigate to={signInPathReturningTo(`${location.pathname}${location.search}`)} replace />;
    }

    return <Outlet />;
};

export default RequireAuth;
