import { CurrentMatchResponse, GameState } from "@/models/entities/Bracket";
import { lobbyPath, showBracketPath } from "@/services/gameRoutes";

// Resolves whether in-match view should redirect based on game state and participant context.
export const resolveInMatchRedirect = (
    gameState: GameState | null,
    currentMatch: CurrentMatchResponse | null,
    playerId: string | null,
    gameId: string
): string | null =>
{
    if (!gameState)
    {
        return null;
    }

    if (gameState === "LOBBY_WAITING")
    {
        return lobbyPath(gameId);
    }

    if (gameState === "BRACKET_VIEW" || gameState === "COMPLETE")
    {
        return showBracketPath(gameId);
    }

    if (!currentMatch)
    {
        return showBracketPath(gameId);
    }

    const isPlayerInCurrentMatch = Boolean(
        playerId && (currentMatch.playerOneId === playerId || currentMatch.playerTwoId === playerId)
    );

    if (!isPlayerInCurrentMatch)
    {
        return showBracketPath(gameId);
    }

    return null;
};
