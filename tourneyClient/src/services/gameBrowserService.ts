import { GameSummary } from "@/models/entities/Bracket";
import { RequestService } from "@/services/RequestService";
import { routeForGameState } from "@/services/gameRoutes";

// What a player can do with one tournament in the browser.
//
// The three cases are genuinely different destinations, not three labels on one
// button: rejoining goes straight back into a running game, joining goes
// through character selection first, and a started game you are not in has
// nowhere to send you at all.
export type GameBrowserAction = "REJOIN" | "JOIN" | "CLOSED";

// Describes the outcome of a host ending their tournament.
//
// A refusal is kept distinct from a failure because they need different words:
// "you do not host this" is a fact the player can act on, "we could not reach
// the server" is something to retry.
export type EndTournamentOutcome =
    | { kind: "ended" }
    | { kind: "notHost" }
    | { kind: "notFound" }
    | { kind: "failed" };

// Asks the server which tournaments exist and how they relate to this user.
export const fetchActiveGames = async (): Promise<GameSummary[]> =>
{
    const games = await RequestService<"getActiveGames", never, GameSummary[]>("getActiveGames");

    if (!Array.isArray(games))
    {
        return [];
    }

    return games;
};

// Resolves what this user can do with a tournament they are looking at.
export const resolveGameAction = (game: GameSummary): GameBrowserAction =>
{
    if (game.hasJoined)
    {
        return "REJOIN";
    }

    // A bracket that has already been drawn cannot take another entrant —
    // AddPlayer refuses it server-side — so the browser says so rather than
    // walking someone through character selection for a join that will fail.
    if (game.state !== "LOBBY_WAITING")
    {
        return "CLOSED";
    }

    return "JOIN";
};

// Resolves where selecting a tournament should take this user.
//
// Someone already in the game goes wherever the server says their game is,
// which is the same rule the reconnect path uses. Everyone else goes to the
// join screen with the id already in the URL — the whole point of the browser
// is that nobody has to read a GUID off someone else's phone.
export const gameBrowserDestination = (game: GameSummary): string | null =>
{
    const action = resolveGameAction(game);

    if (action === "REJOIN")
    {
        return routeForGameState(game.state, game.gameId);
    }

    if (action === "JOIN")
    {
        return `/joinTourney?gameId=${encodeURIComponent(game.gameId)}`;
    }

    return null;
};

// Orders tournaments so the ones a player can act on are at the top.
//
// On a phone the list is read from the top and abandoned quickly, so games the
// player already belongs to come first, then ones still taking entrants, then
// everything they can only look at. Newest first within each band, because the
// game somebody just made is almost always the one being talked about.
export const sortGamesForBrowsing = (games: GameSummary[]): GameSummary[] =>
{
    const actionRank: Record<GameBrowserAction, number> = {
        REJOIN: 0,
        JOIN: 1,
        CLOSED: 2
    };

    return [...games].sort((firstGame, secondGame) =>
    {
        const rankDifference = actionRank[resolveGameAction(firstGame)] - actionRank[resolveGameAction(secondGame)];
        if (rankDifference !== 0)
        {
            return rankDifference;
        }

        return Date.parse(secondGame.createdUtc) - Date.parse(firstGame.createdUtc);
    });
};

// Describes a tournament's state in words a player reads rather than an enum.
export const describeGameState = (game: GameSummary): string =>
{
    if (game.state === "LOBBY_WAITING")
    {
        return "Waiting in lobby";
    }

    if (game.state === "COMPLETE")
    {
        return "Finished";
    }

    return "In progress";
};

// Asks the server to end a tournament on the host's behalf.
export const endTournament = async (gameId: string): Promise<EndTournamentOutcome> =>
{
    try
    {
        await RequestService<"endGame", never, string>("endGame", { routeParams: { gameId } });
        return { kind: "ended" };
    }
    catch (error)
    {
        const message = error instanceof Error ? error.message : "";

        // The server distinguishes "not yours" from "not there", and so does the
        // banner the player ends up reading.
        if (message.includes("403"))
        {
            return { kind: "notHost" };
        }

        if (message.includes("404"))
        {
            return { kind: "notFound" };
        }

        return { kind: "failed" };
    }
};
