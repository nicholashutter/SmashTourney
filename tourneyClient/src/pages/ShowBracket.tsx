import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";

const ShowBracket = () =>
{

    //this value should be calculated somewhere in the game object and sent back?
    const GAMES_LEFT = 5;
    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>Current Score</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    {
                        //show bracket components here once data comes from api about bracket structure
                        //parentBracket component here and the child brackets will be dynamically rendered
                        //based on drawService
                    }
                </div>
            </div>
        </div>

    );
}
export default ShowBracket;