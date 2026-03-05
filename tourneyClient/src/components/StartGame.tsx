import BasicHeading from "@/components/HeadingOne";

// Renders the startup splash while the first bracket view is prepared.
const StartGame = () =>
{
    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
                <title>Starting Game!</title>
                <BasicHeading headingText="Starting Game. Please Wait..." headingColors="white" />
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                </div>
            </div>
        </div>);

}
export default StartGame;