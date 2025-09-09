import BasicHeading from "@/components/BasicHeading";
import HeadingTwo from "@/components/HeadingTwo";
import SubmitButton from "@/components/SubmitButton";

const InMatch = () =>
{
    const PLAYER_ONE = "nicholas";
    const PLAYER_TWO = "easton";
    return (

        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>{PLAYER_ONE}VS. {PLAYER_TWO}</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                    <BasicHeading headingText={`${PLAYER_ONE} VS. ${PLAYER_TWO}`} headingColors="white" />
                    <HeadingTwo headingText="Who Won?" />
                    <SubmitButton buttonLabel={`${PLAYER_ONE}`} onSubmit={() =>
                    {
                        //voting service needs to be written here and called
                    }
                    } />
                    <SubmitButton buttonLabel={`${PLAYER_TWO}`} onSubmit={() =>
                    {
                        //voting service needs to be written here and called
                    }
                    } />
                    <SubmitButton buttonLabel="Lock in Vote." onSubmit={() =>
                    {
                        //this button should be invisible until the handler for one of the
                        //above buttons is called
                    }
                    } />
                </div>
            </div>
        </div>

    );
}
export default InMatch;