import BasicHeading from "@/components/HeadingOne";

/*

this page needs to get the gameId from useContext
by this point in the application flow, there must be a gameId

then requestService to startGame endpoint with gameId as the payload
*/






const StartGame = () =>
{
    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 "> {/* max width is 90 percent of parent (viewport) inner flexbox to center content and text */}
                <title>Starting Game!</title>
                <BasicHeading headingText="Starting Game. Please Wait..." headingColors="white" />
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>
                </div>
            </div>
        </div>);

}
export default StartGame;