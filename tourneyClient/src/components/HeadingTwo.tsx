type AppProps =
    {
        headingText: string;
    };

const HeadingTwo = ({ headingText }: AppProps) =>
{
    return (
        <label className="m-4  rounded shadow-md">
            {headingText}
        </label>
    );

}

export default HeadingTwo; 