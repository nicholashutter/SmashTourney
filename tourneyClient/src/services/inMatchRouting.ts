import { CurrentMatchResponse, GameState } from "@/models/entities/Bracket";

// Resolves whether in-match view should redirect based on game state and participant context.
export const resolveInMatchRedirect = (
    gameState: GameState | null,
    currentMatch: CurrentMatchResponse | null,
    playerId: string | null
): string | null =>
{
    if (!gameState)
    {
        return null;
    }

    if (gameState === "LOBBY_WAITING")
    {
        return "/lobby";
    }

    if (gameState === "BRACKET_VIEW" || gameState === "COMPLETE")
    {
        return "/showBracket";
    }

    if (!currentMatch)
    {
        return "/showBracket";
    }

    const isPlayerInCurrentMatch = Boolean(
        playerId && (currentMatch.playerOneId === playerId || currentMatch.playerTwoId === playerId)
    );

    if (!isPlayerInCurrentMatch)
    {
        return "/showBracket";
    }

    return null;
};
