export const headingColors =
    {
        white: "white",
        black: "black"
    } as const;

export type headingColorsType = typeof headingColors[keyof typeof headingColors];


type AppProps = {
    headingText: string;
    headingColors: headingColorsType;
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

