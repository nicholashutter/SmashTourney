import React from "react";
import { useState } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import PersistentConnection from "../services/PersistentConnection"
import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/BasicHeading";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";
import { CharacterName } from "@/models/Enums/CharacterName";
import
{
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"




const JoinTourney = () =>
{

  //from react router for navigation without reloading
  const navigate = useNavigate();

  //player should enter
  const [sessionCode, setSessionCode] = useState("");
  //player should enter
  const [playerName, setPlayerName] = useState("");

  //player will choose through dropdown components
  const [currentCharacter, setCurrentCharacter] = useState("");

  //create instance of signalR to send lobby updates to other users
  const lobbyConnection = new PersistentConnection();

  const handleSessionCode = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setSessionCode(e.target.value);
  }

  const handlePlayerNameChange = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    setPlayerName(e.target.value);
  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Join Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Join Room" headingColors="white" />
          <BasicInput labelText="Session Code:" htmlFor="sessionCode"
            id="sessionCode" name="sessionCode" value={sessionCode} onChange={handleSessionCode} />
          <BasicInput labelText="Enter Player Name:" htmlFor="playerName"
            id="playerName" name="playerName" value={playerName} onChange={handlePlayerNameChange} />
          <DropdownMenu>
            <DropdownMenuTrigger>Choose Your Fighter</DropdownMenuTrigger>
            <DropdownMenuContent>
              {
                //need to do a map function here something like in the playerList component
              }
              <DropdownMenuItem>Mario</DropdownMenuItem>

            </DropdownMenuContent>
          </DropdownMenu>
          <SubmitButton buttonLabel="Join Room" onSubmit={async () =>
          {
            RequestService(
              "addPlayers",
              {
                body:
                {
                  id: "",
                  userId: "",
                  displayName: playerName,
                  currentScore: 0,
                  currentRound: 0,
                  currentCharacter: currentCharacter,
                  currentGameId: sessionCode
                }
              }
            )
            await lobbyConnection.notifyOthers(sessionCode);
            await lobbyConnection.updateOthers(playerName);
            window.alert("submission success");

            navigate("/lobby");
          }} />
          <BasicButton buttonLabel="Return to Main Menu" href="/" />

        </div>
      </div>
    </div >
  );
};

export default JoinTourney;
