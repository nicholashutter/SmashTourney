type AppProps = {
    buttonLabel: string;
    onSubmit: (e: React.MouseEvent<HTMLAnchorElement>) => void;
};

const SubmitButton = ({ buttonLabel, onSubmit }: AppProps) =>
{


    return (
        <a href="#"
            onClick={(e) =>
            {
                e.preventDefault();
                onSubmit(e);
            }}
            className="p-2 m-2 shrink bg-green-500 hover:bg-green-700 text-white font-bold rounded shadow-md transition duration-300 ease-in-out focus:outline-none focus:ring-2 focus:ring-green-400 focus:ring-opacity-75">
            {buttonLabel}
        </a>
    );
};

export default SubmitButton;