import BasicHeading from "@/components/HeadingOne";
import PageShell from "@/components/PageShell";
import { Player } from "@/models/entities/Player";

// Renders the pre-match versus splash between round transitions.
const Vs = () =>
{

    const PLAYER_ONE = {} as Player;
    const PLAYER_TWO = {} as Player;
    return (

        <PageShell pageTitle={`${PLAYER_ONE.displayName || "Player One"}VS. ${PLAYER_TWO.displayName || "Player Two"}`}>
            <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                <BasicHeading headingText={`${PLAYER_ONE.displayName || "Player One"} VS. ${PLAYER_TWO.displayName || "Player Two"}`} headingColors="white" />
            </div>
        </PageShell>

    );
}

export default Vs;