import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";

const ShowBracket = () =>
{

    /*
        [playersInGame, setPlayersInGame] = useState([]:Player)
        get the gameId from useGameData()
        const response = await requestSVC( GetPlayersInGame() ); 
        validateResponse
        if (validateResponse)
            setPlayers(response)
    */
    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>Current Score</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    {
                        /*
                            <ParentBracket />
                            the children of parentBracket should dynamically be picked based on
                            size of players []
                        */
                    }
                </div>
            </div>
        </div>

    );
}
export default ShowBracket;