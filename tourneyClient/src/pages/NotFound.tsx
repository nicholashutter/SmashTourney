
import BasicButton from "@/components/BasicButton";
import BasicHeading from "@/components/BasicHeading";


const NotFound = () =>
{

    return (
        <div className="flex flex-col items-center justify-center h-dvh w-dvw">
            <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
                <title>Not Found</title>
                <div className='shrink flex flex-col text-2xl p-4 m-4 '>

                    <BasicHeading headingText="The page you are looking for cannot be found." headingColors="white" />
                    <BasicButton buttonLabel="Return Home" href="/" />

                </div>
            </div>
        </div>
    );
};

export default NotFound;