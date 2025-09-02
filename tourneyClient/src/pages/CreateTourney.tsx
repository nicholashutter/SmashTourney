import { useState } from "react";

import BasicHeading from "../components/BasicHeading";
import BasicInput from "../components/BasicInput";
import SubmitButton from "../components/SubmitButton";

function handleSubmit()
{
  window.alert("Submission Success");
}



const CreateTourney = () =>
{
  const sessionCode = "";

  const [numPlayers, setNumPlayers] = useState("");
  const [gameType, setGameType] = useState("single");

  const handleMaxPlayers = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setNumPlayers(e.target.value);
  }

  const handleSelectGameType = (e: React.ChangeEvent<HTMLSelectElement>) =>
  {
    setGameType(e.target.value);
  }

  const swapGameType = (gameType: boolean) =>
  {
    return gameType ? "single" : "double";
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <BasicHeading headingText="Create Tourney" />
      <BasicInput labelText="Session Code:" name="sessionCode" htmlFor="sessionCode" id="sessionCode" value={sessionCode} onChange={() => { }} />

      <label htmlFor="ruleset">Ruleset:</label>
      <select id="ruleset" name="ruleset" onChange={handleSelectGameType}>
        <option >Single Elimination</option>
        <option >Double Elimination</option>
      </select>
      <BasicInput labelText="Max Players:" htmlFor="maxPlayers" name="maxPlayers" id="maxPlayers" value={numPlayers} onChange={handleMaxPlayers} />
      <SubmitButton buttonLabel="Create Tourney" onSubmit={handleSubmit} />
    </div>
  );
}

export default CreateTourney;
