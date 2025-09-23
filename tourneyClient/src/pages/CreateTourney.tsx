import { useState } from "react";

import BasicHeading from "@/components/BasicHeading";
import BasicInput from "@/components/BasicInput";
import SubmitButton from "@/components/SubmitButton";
import { RequestService } from "@/services/RequestService";
import { useNavigate } from 'react-router';
import { useGameId } from "@/components/GameIdContext";





const CreateTourney = () =>
{

  //dynamic import react router useNavigate
  const navigate = useNavigate();

  //setup store for gameId after creation
  const { setId } = useGameId();

  //variables for users selections
  const [numPlayers, setNumPlayers] = useState("");
  const [gameType, setGameType] = useState(false);

  //handle max player selection
  const handleMaxPlayers = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setNumPlayers(e.target.value);
  }

  //handle select game selection
  const handleSelectGameType = async () =>
  {
    setGameType(!gameType);
  }

  //handle submit selection
  const HandleSubmit = async () =>
  {


    //call request service and provide no body object since our api does not need a body for createGame
    const gameId: string = await RequestService("createGame");

    if (typeof gameId != "string")
    {
      console.error("Invalid API response");
    }

    //use set function created with at top level of component 
    setId(gameId);

    window.alert("submission success");
    //force navigation without user intervention upon request completion and alert dismissal
    navigate("/lobby");
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Create Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Create Tourney" headingColors="white" />

          <label className="text-md" htmlFor="ruleset">Ruleset:</label>
          <select className="text-sm p-4 m-4" id="ruleset" name="ruleset" onChange={handleSelectGameType}>
            <option className="text-black">Single Elimination</option>
            <option className="text-black">Double Elimination</option>
          </select>
          <BasicInput labelText="Max Players:" htmlFor="maxPlayers" name="maxPlayers" id="maxPlayers" value={numPlayers} onChange={handleMaxPlayers} />
          <SubmitButton buttonLabel="Create Tourney" onSubmit={HandleSubmit} />
        </div>
      </div>
    </div>
  );
}

export default CreateTourney;
