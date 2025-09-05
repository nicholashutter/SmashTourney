import ParentBracket from "./ParentBracket";
import TwoPlayerBracket from '../../components/brackets/TwoPlayerBracket';

const TwoPlayers = () =>
{
    return (
        <>
            <ParentBracket children={<TwoPlayerBracket/>} />
        </>
    );
}

export default TwoPlayers; 