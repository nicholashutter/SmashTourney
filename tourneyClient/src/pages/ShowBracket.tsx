
import DrawWinnersBracket from "@/components/brackets/DynamicBracket";


const ShowBracket = () =>
{



    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw px-2"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white w-[96vw] h-[92dvh]"> {/* explicit size so bracket svg can fill screen */}
                <title>Current Score</title>
                <div className='flex-1 min-h-0 w-full p-4'>
                    <DrawWinnersBracket numPlayers={16} />
                </div>
            </div>
        </div>

    );
}
export default ShowBracket;