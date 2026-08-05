import { GameState } from "@/models/entities/Bracket";

// Builds the in-game route paths, all of which carry the game id.
//
// The three game screens used to sit at fixed paths, which meant the address
// bar said nothing about which tournament you were in. That was fine while the
// browser tab held the session and fatal the moment it did not: reopening the
// URL landed you on a page with no idea what game it was showing. Putting the
// id in the path makes the URL the durable thing — it can be reopened, shared,
// or restored after the tab is gone.

export const lobbyPath = (gameId: string): string => `/lobby/${gameId}`;

export const inMatchPath = (gameId: string): string => `/inMatch/${gameId}`;

export const showBracketPath = (gameId: string): string => `/showBracket/${gameId}`;

export const tourneyMenuPath = (): string => "/tourneyMenu";

// Resolves the screen a player belongs on for a given server game state.
//
// The server is the authority on where a returning player goes. Keeping the
// mapping in one place is what stops the lobby, the bracket and the match
// screen from each having their own opinion about it.
export const routeForGameState = (state: GameState, gameId: string): string =>
{
    if (state === "LOBBY_WAITING")
    {
        return lobbyPath(gameId);
    }

    if (state === "IN_MATCH_ACTIVE")
    {
        return inMatchPath(gameId);
    }

    return showBracketPath(gameId);
};
