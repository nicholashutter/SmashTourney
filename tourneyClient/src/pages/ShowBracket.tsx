
import { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import DrawWinnersBracket from "@/components/brackets/DynamicBracket";
import { useGameData } from "@/hooks/useGameData";
import { RequestService } from "@/services/RequestService";
import { BracketSnapshotResponse } from "@/models/entities/Bracket";
import { SERVER_ERROR } from "@/constants/AppConstants";


const ShowBracket = () =>
{
    const navigate = useNavigate();
    const { gameId, setGameStarted } = useGameData();
    const [snapshot, setSnapshot] = useState<BracketSnapshotResponse | null>(null);

    useEffect(() =>
    {
        const loadSnapshot = async () =>
        {
            if (!gameId)
            {
                return;
            }

            try
            {
                const result = await RequestService<"getBracket", never, BracketSnapshotResponse>("getBracket", {
                    routeParams: { gameId }
                });
                setSnapshot(result);
            }
            catch (error)
            {
                console.error("Failed to load bracket snapshot", error);
                window.alert(SERVER_ERROR("Get Bracket"));
            }
        };

        loadSnapshot();
    }, [gameId]);

    useEffect(() =>
    {
        setGameStarted(true);

        const moveToMatch = window.setTimeout(() =>
        {
            navigate("/inMatch", { replace: true });
        }, 3000);

        return () =>
        {
            window.clearTimeout(moveToMatch);
        };
    }, [navigate, setGameStarted]);



    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw px-2"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white w-[96vw] h-[92dvh]"> {/* explicit size so bracket svg can fill screen */}
                <title>Current Score</title>
                <div className='flex-1 min-h-0 w-full p-4'>
                    <DrawWinnersBracket numPlayers={snapshot?.players?.length ?? 16} />
                </div>
            </div>
        </div>

    );
}
export default ShowBracket;