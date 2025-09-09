import BasicHeading from "@/components/BasicHeading";
import HeadingTwo from "@/components/HeadingTwo";

const RoundScore = () =>
{
    const GAMES_LEFT = 5;
    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>Current Score</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText="Current Round:" headingColors="white" />
                    <HeadingTwo headingText="Winners:" />
                    {
                        //here should be a call to displayWinnersService
                    }
                    <HeadingTwo headingText="Losers:" />
                    {
                        //here should be a call to displayLosersService
                    }
                    <BasicHeading headingText={`Games Left This Round:${GAMES_LEFT}`} headingColors="white" />
                </div>
            </div>
        </div>

    );
}
export default RoundScore;