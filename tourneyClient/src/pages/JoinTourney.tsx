import React from "react";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import { useGameData } from "@/components/GameIdContext";
import { validateInput } from "@/services/ValidationService";
import { INVALID_CHARACTERS, SUBMIT_SUCCESS } from "@/constants/AppConstants";
import { Character } from "@/models/entities/Character.ts";
import { Player } from "@/models/entities/Player";
import { v4 as uuidv4 } from "uuid";
import PersistentConnection from "../services/PersistentConnection"
import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";
import
{
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"


/* Ready for E2E testing */

const JoinTourney = () =>
{

  //from react router for navigation without reloading
  const navigate = useNavigate();

  const { setGameId: setGameId, gameId: gameId, setPlayerId: setPlayerId, playerId: playerId } = useGameData();
  //player should enter
  const [displayName, setDisplayName] = useState("");

  //array will be loaded with characters using dynamic import.meta.glob provided by vite
  const [characters, setCharacters] = useState<Character[]>([]);

  //player will choose through dropdown components
  const [currentCharacter, setCurrentCharacter] = useState({} as Character);

  //create instance of signalR to send lobby updates to other users
  const lobbyConnection = new PersistentConnection();



  useEffect(() =>
  {
    //Get all matching character module paths and their import functions
    //vite docs detail this behavior 
    //characterModuleMap will be an object of import urls and callback functions
    const characterModuleMap = import.meta.glob("../models/entities/Characters/*.ts");

    //define an async function to load all character modules
    const fetchAllCharacters = async () =>
    {
      //call each import function to dynamically load the module
      const modulePromises = Object.values(characterModuleMap).map((dynamicImport) => dynamicImport());

      //wait for all modules to finish loading
      const resolvedModules = await Promise.all(modulePromises);

      //extract the default export (the character object) from each module
      const characterObjects = resolvedModules.map((module) => module.default);

      //save the array of character objects to state
      setCharacters(characterObjects);
    };

    //Run the async loader function
    fetchAllCharacters();
  });

  const gameIdHandler = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    const validateGameId = validateInput(e.target.value);
    if (validateGameId)
    {
      //custom useContext for gameId
      setGameId(e.target.value);
    }
    else
    {
      window.alert(INVALID_CHARACTERS("GameId"));
    }

  }

  const displayNameHandler = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    const validateDisplayName = validateInput(e.target.value);
    if (validateDisplayName)
    {
      setDisplayName(displayName);
    }
    else
    {
      window.alert(INVALID_CHARACTERS("Display Name"));
    }

  }

  const submitHandler = async () =>
  {
    if (gameId)
    {
      //check if the playerId is undefined or null if it is then set it here

      if (!playerId)
      {
        setPlayerId(uuidv4());
      }
      const player: Player =
        {
          Id: playerId,
          displayName: displayName,
          currentScore: 0,
          currentRound: 0,
          currentCharacter: currentCharacter,
          currentGameId: gameId
        } as Player;
      await RequestService(
        "addPlayers",
        {
          body:
          {
            player
          },
          routeParams:
          {
            gameId
          }
        }
      );
      await lobbyConnection.updateOthers(displayName);
      window.alert(SUBMIT_SUCCESS("Join Tourney"));

      navigate("/lobby");
    }
    else
    {
      window.alert(INVALID_CHARACTERS("GameId"));
    }

  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Join Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Join Room" headingColors="white" />
          <BasicInput labelText="Session Code:" htmlFor="sessionCode"
            id="gameId" name="gameId" value={gameId!} onChange={gameIdHandler} />
          <BasicInput labelText="Enter Player Name:" htmlFor="playerName"
            id="displayName" name="displayName" value={displayName} onChange={displayNameHandler} />
          <DropdownMenu>
            <DropdownMenuTrigger className="shrink p-2 m-2 bg-white hover:ring-2 hover:ring-green-400 text-black  font-bold rounded shadow-md 
      transition duration-300 ease-in-out focus:outline-none focus:ring-2 focus:ring-green-400 focus:ring-opacity-75">{currentCharacter.characterName || "Choose Your Fighter"}</DropdownMenuTrigger>
            <DropdownMenuContent>
              {characters.map((Character, index) => (
                <DropdownMenuItem key={index}
                  onSelect={() => setCurrentCharacter(Character)}>
                  {Character.characterName}
                  {/* 
                  <br />
                  {Character.archetype}<br />
                  {Character.fallSpeed}<br />
                  {Character.tierPlacement}<br />
                  {Character.weightClass}
                  
                  */ }

                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
          <SubmitButton buttonLabel="Join Room" onSubmit={submitHandler} />
          <BasicButton buttonLabel="Return to Main Menu" href="/" />

        </div>
      </div>
    </div >
  );
};

export default JoinTourney;
