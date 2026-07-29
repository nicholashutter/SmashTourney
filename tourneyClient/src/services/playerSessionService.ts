import { GameState } from "@/models/entities/Bracket";
import { RequestService } from "@/services/RequestService";

// Describes a returning player's place in a game, as resolved by the server.
export type PlayerSessionResponse = {
    gameId: string;
    playerId: string;
    displayName: string;
    isHost: boolean;
    state: GameState;
    gameStarted: boolean;
};

// Describes the outcome of trying to resume a session from a game id.
//
// "notParticipant" is deliberately not an error. Someone opening a shared link
// for a game they have not joined is doing something reasonable, and the right
// answer is to send them to the join screen, not to show them a failure.
export type ResumeOutcome =
    | { kind: "resumed"; session: PlayerSessionResponse }
    | { kind: "notParticipant" }
    | { kind: "failed" };

// Asks the server who the signed-in user is within a game.
export const resumePlayerSession = async (gameId: string): Promise<ResumeOutcome> =>
{
    try
    {
        const session = await RequestService<"getPlayerSession", never, PlayerSessionResponse>(
            "getPlayerSession",
            { routeParams: { gameId } }
        );

        return { kind: "resumed", session };
    }
    catch (error)
    {
        const message = error instanceof Error ? error.message : "";

        // A 404 is the server saying this user has no player in this game,
        // which is a different situation from the request failing.
        if (message.includes("404"))
        {
            return { kind: "notParticipant" };
        }

        return { kind: "failed" };
    }
};
