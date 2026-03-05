import BasicHeading from "@/components/HeadingOne";
import { Player } from "@/models/entities/Player";

// Renders the pre-match versus splash between round transitions.
const Vs = () =>
{

    const PLAYER_ONE = {} as Player;
    const PLAYER_TWO = {} as Player;
    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
                <title>{PLAYER_ONE.displayName || "Player One"}VS. {PLAYER_TWO.displayName || "Player Two"}</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText={`${PLAYER_ONE.displayName || "Player One"} VS. ${PLAYER_TWO.displayName || "Player Two"}`} headingColors="white" />
                </div>
            </div>
        </div>

    );
}

export default Vs;