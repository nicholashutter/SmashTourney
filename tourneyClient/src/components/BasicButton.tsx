import { Link } from "react-router";

type AppProps = 
{
    buttonLabel: string;
    href: string;
};


const BasicButton = ({ buttonLabel, href }: AppProps) =>
{
    return (<Link
        to={href}
        className="shrink p-2 m-2 bg-green-500 hover:bg-green-700 text-white font-bold rounded shadow-md 
      transition duration-300 ease-in-out focus:outline-none focus:ring-2 focus:ring-green-400 focus:ring-opacity-75"
    >
        {buttonLabel}
    </Link>);
}

export default BasicButton;