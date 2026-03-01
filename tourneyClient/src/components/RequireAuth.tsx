import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router";
import { RequestService } from "@/services/RequestService";

type AuthState = "loading" | "authenticated" | "unauthenticated";

const RequireAuth = () =>
{
    const [authState, setAuthState] = useState<AuthState>("loading");

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

    if (authState === "unauthenticated")
    {
        return <Navigate to="/" replace />;
    }

    return <Outlet />;
};

export default RequireAuth;
