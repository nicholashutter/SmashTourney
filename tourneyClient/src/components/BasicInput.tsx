type AppProps = {
    htmlFor: string;
    name: string;
    id: string;
    labelText: string;
    value: string; 
    onChange:(e:React.ChangeEvent<HTMLInputElement>)=> void;
};


const BasicInput = ({ htmlFor, name, id, labelText, value, onChange }: AppProps) => 
{
    return (
        <>
            <label htmlFor={htmlFor} >{labelText}</label>
            <input className="shrink bg-white m-5 rounded shadow-md " type="text" id={id} name={name} value={value} onChange={onChange} />
        </>);
}

export default BasicInput;