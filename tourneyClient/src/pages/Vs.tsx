import BasicHeading from "@/components/BasicHeading";
import { useParams } from "react-router";

/*

this needs to call /Games/StartMatch using request service

await the response then display the playernames in the title and heading spots below


*/


const Vs = () =>
{
    const params = useParams();
    const PLAYER_ONE = params.playerOne;
    const PLAYER_TWO = params.playerTwo;
    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>{PLAYER_ONE}VS. {PLAYER_TWO}</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText={`${PLAYER_ONE} VS. ${PLAYER_TWO}`} headingColors="white" />
                </div>
            </div>
        </div>

    );
}

export default Vs;