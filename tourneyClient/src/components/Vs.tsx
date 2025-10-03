import BasicHeading from "@/components/HeadingOne";
import { Player } from "@/models/entities/Player";
import { useParams } from "react-router";

/*

    this will be a loading screen component displayed briefly before 
    inMatch component

    Should be shown every time the api sends two new players
    For a new round
    
*/


const Vs = () =>
{
    
    const PLAYER_ONE = {} as Player;
    const PLAYER_TWO = {} as Player;
    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>{PLAYER_ONE.displayName||"Player One"}VS. {PLAYER_TWO.displayName||"Player Two"}</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText={`${PLAYER_ONE.displayName||"Player One"} VS. ${PLAYER_TWO.displayName||"Player Two"}`} headingColors="white" />
                </div>
            </div>
        </div>

    );
}

export default Vs;