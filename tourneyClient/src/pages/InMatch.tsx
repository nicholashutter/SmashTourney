import { useCallback, useEffect, useMemo, useState } from "react";
import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton from "@/components/SubmitButton";
import { useGameData } from "@/hooks/useGameData";
import { RequestService } from "@/services/RequestService";
import { BracketSnapshotResponse, CurrentMatchResponse, ReportMatchRequest } from "@/models/entities/Bracket";
import { SERVER_ERROR, SUBMIT_SUCCESS } from "@/constants/AppConstants";

/*
    possibly using useGameData
    if inMatch being shown first time
        show StartGame
    else
        show Vs briefly
        then show this component for remainder of match

*/
const InMatch = () =>
{
    const { gameId } = useGameData();
    const [currentMatch, setCurrentMatch] = useState<CurrentMatchResponse | null>(null);
    const [snapshot, setSnapshot] = useState<BracketSnapshotResponse | null>(null);
    const [selectedWinnerId, setSelectedWinnerId] = useState<string | null>(null);

    const playersById = useMemo(() =>
    {
        return new Map((snapshot?.players ?? []).map((player) => [player.playerId, player.displayName]));
    }, [snapshot]);

    const playerOneName = currentMatch?.playerOneId
        ? (playersById.get(currentMatch.playerOneId) ?? "Player One")
        : "Player One";
    const playerTwoName = currentMatch?.playerTwoId
        ? (playersById.get(currentMatch.playerTwoId) ?? "Player Two")
        : "Player Two";

    const loadMatchData = useCallback(async () =>
    {
        if (!gameId)
        {
            return;
        }

        try
        {
            const [current, bracket] = await Promise.all([
                RequestService<"getCurrentMatch", never, CurrentMatchResponse>("getCurrentMatch", {
                    routeParams: { gameId }
                }).catch(() => null),
                RequestService<"getBracket", never, BracketSnapshotResponse>("getBracket", {
                    routeParams: { gameId }
                }).catch(() => null)
            ]);

            setCurrentMatch(current);
            setSnapshot(bracket);
            setSelectedWinnerId(null);
        }
        catch (error)
        {
            console.error("Failed to load in-match data", error);
        }
    }, [gameId]);

    useEffect(() =>
    {
        loadMatchData();
    }, [loadMatchData]);

    const handleLockVote = async () =>
    {
        if (!gameId || !currentMatch || !selectedWinnerId)
        {
            return;
        }

        try
        {
            const payload: ReportMatchRequest = {
                matchId: currentMatch.matchId,
                winnerPlayerId: selectedWinnerId
            };

            await RequestService<"reportMatch", ReportMatchRequest, void>("reportMatch", {
                body: payload,
                routeParams: { gameId }
            });

            window.alert(SUBMIT_SUCCESS("Report Match"));
            await loadMatchData();
        }
        catch (error)
        {
            console.error("Failed to report match", error);
            window.alert(SERVER_ERROR("Report Match"));
        }
    };

    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>{playerOneName}VS. {playerTwoName}</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText={`${playerOneName} VS. ${playerTwoName}`} headingColors="white" />
                    {currentMatch ? (
                        <>
                            <HeadingTwo headingText="Who Won?" />
                            <SubmitButton buttonLabel={`${playerOneName}${selectedWinnerId === currentMatch.playerOneId ? " ✓" : ""}`} onSubmit={() =>
                            {
                                setSelectedWinnerId(currentMatch.playerOneId);
                            }
                            } />
                            <SubmitButton buttonLabel={`${playerTwoName}${selectedWinnerId === currentMatch.playerTwoId ? " ✓" : ""}`} onSubmit={() =>
                            {
                                setSelectedWinnerId(currentMatch.playerTwoId);
                            }
                            } />
                            {selectedWinnerId && (
                                <SubmitButton buttonLabel="Lock in Vote." onSubmit={handleLockVote} />
                            )}
                        </>
                    ) : (
                        <HeadingTwo headingText="Waiting for next match..." />
                    )}
                    {!currentMatch && (
                        <SubmitButton buttonLabel="Refresh Match" onSubmit={loadMatchData} />
                    )}
                </div>
            </div>
        </div>

    );
}
export default InMatch;