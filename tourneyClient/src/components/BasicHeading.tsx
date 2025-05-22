type AppProps = {
    headingText: string;
};


const BasicHeading = ({ headingText }: AppProps) =>
{
    return (
        <>
            <h2 className="bg-black/25 rounded shadow-md text-6xl p-2 my-6 ">{headingText}</h2>
        </>
    );
}

export default BasicHeading;

