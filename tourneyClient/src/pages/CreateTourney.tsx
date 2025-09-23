import { useState } from "react";

import BasicHeading from "@/components/BasicHeading";
import BasicInput from "@/components/BasicInput";
import SubmitButton from "@/components/SubmitButton";
import { RequestService } from "@/services/RequestService";
import { useNavigate } from 'react-router';
import { useGameId } from "@/components/GameIdContext";
import HeadingTwo from "@/components/HeadingTwo";




const CreateTourney = () =>
{

  const MAX_SUPPORTED_PLAYERS = 128;

  //dynamic import react router useNavigate
  const navigate = useNavigate();

  //setup store for gameId after creation
  const { setId } = useGameId();

  //variables for users selections
  //will set this with dropdown component with preset values
  const [numPlayers, setNumPlayers] = useState("");

  //gameType true is double elimination
  //gameType false is single elimination
  //will set this with an enum like object
  const [gameType, setGameType] = useState(false);

  //handle max player selection
  const handleMaxPlayers = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setNumPlayers(e.target.value);
  }

  //handle select game selection
  const handleSelectGameType = async (e: React.ChangeEvent<HTMLSelectElement>) =>
  {
    //get value of event from select element
    const selected = e.target.value;

    //set gameType based on selection
    switch (selected)
    {
      case "Single Elimination":
        setGameType(false);
        console.log("Single Elimination implementation under construction");
        break;
      case "Double Elimination":
        setGameType(true);
        break
      default:
        console.log("Invalid game type selection");
        break;
    }
  }

  //handle submit selection
  const handleSubmit = async () =>
  {

    //refactor this into validation function/object
    const numberOfPlayers = parseInt(numPlayers);
    if (numberOfPlayers > MAX_SUPPORTED_PLAYERS || numberOfPlayers < 0)
    {
      window.alert(`Total players must be greater than zero and less than ${MAX_SUPPORTED_PLAYERS}`);
    }

    else
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
          <HeadingTwo headingText={`Enter Total Players (Up to ${MAX_SUPPORTED_PLAYERS})`} />
          <BasicInput labelText="" htmlFor="maxPlayers" name="maxPlayers" id="maxPlayers" value={numPlayers} onChange={handleMaxPlayers} />
          <SubmitButton buttonLabel="Create Tourney" onSubmit={handleSubmit} />
        </div>
      </div>
    </div>
  );
}

export default CreateTourney;
