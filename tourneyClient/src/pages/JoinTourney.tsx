import React from "react";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
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
import { validateInput } from "@/services/ValidationService";
import { INVALID_CHARACTERS } from "@/constants/StatusMessages";
import { Character } from "@/models/entities/Character.ts";





const JoinTourney = () =>
{

  //from react router for navigation without reloading
  const navigate = useNavigate();

  //player should enter
  const [gameId, setGameId] = useState("");
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

    //Define an async function to load all character modules
    const fetchAllCharacters = async () =>
    {
      //Call each import function to dynamically load the module
      const modulePromises = Object.values(characterModuleMap).map((dynamicImport) => dynamicImport());

      //Wait for all modules to finish loading
      const resolvedModules = await Promise.all(modulePromises);

      //Extract the default export (the character object) from each module
      const characterObjects = resolvedModules.map((module) => module.default);

      //Save the array of character objects to state
      setCharacters(characterObjects);
    };

    // Step 7: Run the async loader function
    fetchAllCharacters();
  });

  const gameIdHandler = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    const validateGameId = validateInput(gameId);
    if (validateGameId)
    {
      setGameId(e.target.value);
    }
    else
    {
      window.alert(INVALID_CHARACTERS("GameId"));
    }

  }

  const displayNameHandler = (e: React.ChangeEvent<HTMLInputElement>) =>
  {
    const validateDisplayName = validateInput(displayName);
    if (validateDisplayName)
    {
      setDisplayName(e.target.value);
    }
    else
    {
      window.alert(INVALID_CHARACTERS("Display Name"));
    }

  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Join Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Join Room" headingColors="white" />
          <BasicInput labelText="Session Code:" htmlFor="sessionCode"
            id="sessionCode" name="sessionCode" value={gameId} onChange={gameIdHandler} />
          <BasicInput labelText="Enter Player Name:" htmlFor="playerName"
            id="playerName" name="playerName" value={displayName} onChange={displayNameHandler} />
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
          <SubmitButton buttonLabel="Join Room" onSubmit={async () =>
          {
            await RequestService(
              "addPlayers",
              {
                body:
                {
                  //front end will actually need to generate the playerId
                  id: "",
                  userId: "",
                  //front end will actually need to fetch the userId associated with that player using the claimsPrinciple associated with the cookie
                  //this will end up being handled on the request submission
                  displayName: displayName,
                  currentScore: 0,
                  currentRound: 0,
                  currentCharacter: currentCharacter,
                  currentGameId: gameId
                }
              }
            )
            await lobbyConnection.updateOthers(displayName);
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
