type AppProps = {
    headingText: string;
    headingColors: "white" | "black";
};




const BasicHeading = ({ headingText, headingColors }: AppProps) =>
{
    return (
        <>
            <h2 className={` rounded text-shadow-lg 
                text-5xl p-2 my-6 text-${headingColors} font-[Arial]`}>{headingText}</h2>
        </>
    );
}

export default BasicHeading;

