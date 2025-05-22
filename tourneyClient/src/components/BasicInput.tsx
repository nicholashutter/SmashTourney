type AppProps = {
    htmlFor: string;
    name: string;
    id: string;
    labelText: string;
};


const BasicInput = ({ htmlFor, name, id, labelText }: AppProps) => 
{
    return (
        <>
            <label htmlFor={htmlFor} >{labelText}</label>
            <input className="shrink bg-white m-5 rounded shadow-md " type="text" id={id} name={name} />
        </>);
}

export default BasicInput;