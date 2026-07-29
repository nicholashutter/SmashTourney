import { Navigate } from "react-router";
import { useGameData } from "@/hooks/useGameData";

// Forwards a bare game path onto its game-scoped equivalent.
//
// The game screens moved from "/lobby" to "/lobby/:gameId". Anyone holding an
// old link, or a bookmark made before the move, still lands here — if their
// session knows which game they were in, they carry on; if not, there is
// nothing to reconstruct from and the menu is the honest destination.
const LegacyGameRedirect = ({ buildPath }: { buildPath: (gameId: string) => string }) =>
{
    const { gameId } = useGameData();

    if (!gameId)
    {
        return <Navigate to="/tourneyMenu" replace />;
    }

    return <Navigate to={buildPath(gameId)} replace />;
};

export default LegacyGameRedirect;
