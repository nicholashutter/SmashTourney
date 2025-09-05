import { useState } from "react";

import BasicHeading from "../components/BasicHeading";
import BasicInput from "../components/BasicInput";
import SubmitButton from "../components/SubmitButton";
import { RequestService } from "../utilities/RequestService";
import { useNavigate } from 'react-router';





const CreateTourney = () =>
{
  const sessionCode = "";
  const navigate = useNavigate(); 

  const [numPlayers, setNumPlayers] = useState("");
  const [gameType, setGameType] = useState(false);

  const handleMaxPlayers = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setNumPlayers(e.target.value);
  }

  const handleSelectGameType = () =>
  {
    setGameType(!gameType);
  }

  const handleSubmit = () =>
  {
    window.alert("submission success"); 

    RequestService(
      "createGame", 
      {
        body:
        {

        }
      }
    )

    navigate("/lobby"); 
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Create Tourney</title>
        <BasicHeading headingText="Create Tourney" headingColors="white" />
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
        <BasicInput labelText="Session Code:" name="sessionCode" htmlFor="sessionCode" id="sessionCode" value={sessionCode} onChange={() => { }} />

        <label className="text-md" htmlFor="ruleset">Ruleset:</label>
        <select className="text-sm p-4 m-4" id="ruleset" name="ruleset" onChange={handleSelectGameType}>
          <option className="text-black">Single Elimination</option>
          <option className="text-black">Double Elimination</option>
        </select>
        <BasicInput labelText="Max Players:" htmlFor="maxPlayers" name="maxPlayers" id="maxPlayers" value={numPlayers} onChange={handleMaxPlayers} />
        <SubmitButton buttonLabel="Create Tourney" onSubmit={handleSubmit} />
        </div>
      </div>
    </div>
  );
}

export default CreateTourney;
