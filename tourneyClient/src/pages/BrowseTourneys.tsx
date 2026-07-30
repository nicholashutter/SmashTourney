import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router";
import { AnimatePresence, motion, useReducedMotion } from "framer-motion";
import BasicButton from "@/components/BasicButton";
import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import PageShell from "@/components/PageShell";
import StatusBanner, { StatusMessage } from "@/components/StatusBanner";
import SubmitButton from "@/components/SubmitButton";
import { Card, CardContent } from "@/components/ui/card";
import { GameSummary } from "@/models/entities/Bracket";
import
{
    describeGameState,
    endTournament,
    fetchActiveGames,
    gameBrowserDestination,
    resolveGameAction,
    sortGamesForBrowsing
} from "@/services/gameBrowserService";

// How often the list re-reads the server while somebody is looking at it.
//
// Long enough not to matter, short enough that a game whose state changed on
// somebody else's phone catches up before the person reading this taps a row
// that has moved on.
const REFRESH_INTERVAL_MS = 4000;

// Renders the tournaments this user belongs to.
//
// This is a way back in, not a way to browse. It briefly listed every
// tournament on the server, which is fine on a laptop at a party and wrong on a
// public host — it would hand any account that registered a roll-call of every
// game running. Finding a new tournament happens through a session code or the
// link somebody sent, which is the model anyway: you get given a room, you do
// not go looking through other people's.
const BrowseTourneys = () =>
{
    const navigate = useNavigate();
    const prefersReducedMotion = useReducedMotion();

    const [games, setGames] = useState<GameSummary[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [status, setStatus] = useState<StatusMessage | null>(null);
    const [endingGameId, setEndingGameId] = useState<string | null>(null);

    const loadGames = useCallback(async (): Promise<GameSummary[] | null> =>
    {
        try
        {
            const activeGames = await fetchActiveGames();
            return sortGamesForBrowsing(activeGames);
        }
        catch (error)
        {
            console.error("Failed to load the tournament list", error);
            return null;
        }
    }, []);

    useEffect(() =>
    {
        let isDisposed = false;

        const refreshGames = async (isFirstLoad: boolean) =>
        {
            const loadedGames = await loadGames();

            if (isDisposed)
            {
                return;
            }

            if (loadedGames === null)
            {
                // A failed background poll is left silent on purpose. The list
                // already on screen is still the best answer available, and
                // replacing it with an error every few seconds would make a
                // brief wifi dropout look like the party ended.
                if (isFirstLoad)
                {
                    setStatus({ text: "We could not reach the server. Check your connection.", tone: "error" });
                    setIsLoading(false);
                }

                return;
            }

            setGames(loadedGames);
            setIsLoading(false);
        };

        refreshGames(true);

        const refreshIntervalId = window.setInterval(() =>
        {
            refreshGames(false);
        }, REFRESH_INTERVAL_MS);

        return () =>
        {
            isDisposed = true;
            window.clearInterval(refreshIntervalId);
        };
    }, [loadGames]);

    // Sends the player wherever the selected tournament belongs.
    const handleSelectGame = (game: GameSummary) =>
    {
        const destination = gameBrowserDestination(game);

        if (!destination)
        {
            setStatus({ text: "That tournament has already started, so it is not taking players.", tone: "info" });
            return;
        }

        navigate(destination);
    };

    // Ends a tournament the signed-in user hosts and drops it from the list.
    const handleEndGame = async (game: GameSummary) =>
    {
        if (endingGameId)
        {
            return;
        }

        setEndingGameId(game.gameId);
        setStatus({ text: "Ending tournament...", tone: "info" });

        const outcome = await endTournament(game.gameId);

        if (outcome.kind === "ended")
        {
            setGames((previousGames) => previousGames.filter((current) => current.gameId !== game.gameId));
            setStatus({ text: "Tournament ended.", tone: "success" });
        }
        else if (outcome.kind === "notHost")
        {
            setStatus({ text: "Only the host can end that tournament.", tone: "error" });
        }
        else if (outcome.kind === "notFound")
        {
            setGames((previousGames) => previousGames.filter((current) => current.gameId !== game.gameId));
            setStatus({ text: "That tournament is already gone.", tone: "info" });
        }
        else
        {
            setStatus({ text: "We could not end that tournament. Try again.", tone: "error" });
        }

        setEndingGameId(null);
    };

    // Resolves the label the action button carries for one tournament.
    const resolveActionLabel = (game: GameSummary): string =>
    {
        const action = resolveGameAction(game);

        if (action === "REJOIN")
        {
            return "Return to this tourney";
        }

        if (action === "CLOSED")
        {
            return "Already started";
        }

        return "Join this tourney";
    };

    return (
        <PageShell pageTitle="My Tourneys">
            <div className="shrink flex flex-col text-2xl p-4 m-4">
                <BasicHeading headingText="My Tourneys" headingColors="white" />

                {isLoading && <HeadingTwo headingText="Looking for your tournaments..." />}

                {/* The empty state has to point somewhere, and it cannot point at
                    other people's games any more. Hosting and the session code
                    are the two real ways in, and both are buttons below. */}
                {!isLoading && games.length === 0 && (
                    <HeadingTwo headingText="You are not in any tournaments. Host one, or join with a session code." />
                )}

                <div className="flex flex-col gap-1">
                    {/* Rows animate in and out because this list changes under
                        the player while they read it — lobbies appear, hosts end
                        games. Movement is what makes that legible instead of
                        startling. */}
                    <AnimatePresence initial={false}>
                        {games.map((game) => (
                            <motion.div
                                key={game.gameId}
                                layout
                                initial={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, x: -24 }}
                                animate={{ opacity: 1, x: 0 }}
                                exit={prefersReducedMotion ? { opacity: 0 } : { opacity: 0, x: 24 }}
                                transition={{ type: "spring", stiffness: 380, damping: 30 }}
                            >
                                <Card className="max-w-sm overflow-hidden p-1 m-1">
                                    <CardContent className="flex flex-col gap-1 text-left">
                                        <p className="text-sm font-bold">
                                            {game.bracketMode === "DOUBLE_ELIMINATION" ? "Double Elimination" : "Single Elimination"}
                                        </p>
                                        <p className="text-sm">
                                            {game.playerCount} {game.playerCount === 1 ? "player" : "players"} &middot; {describeGameState(game)}
                                        </p>
                                        {game.isHost && <p className="text-sm font-bold">You host this one</p>}
                                        <SubmitButton
                                            buttonLabel={resolveActionLabel(game)}
                                            onSubmit={() => handleSelectGame(game)}
                                        />
                                        {game.isHost && (
                                            <SubmitButton
                                                buttonLabel={endingGameId === game.gameId ? "Ending..." : "End tourney"}
                                                onSubmit={() => handleEndGame(game)}
                                            />
                                        )}
                                    </CardContent>
                                </Card>
                            </motion.div>
                        ))}
                    </AnimatePresence>
                </div>

                <StatusBanner status={status} />
                <BasicButton buttonLabel="Join With A Session Code" href="/joinTourney" />
                <BasicButton buttonLabel="Return to Main Menu" href="/tourneyMenu" />
            </div>
        </PageShell>
    );
};

export { BrowseTourneys };
