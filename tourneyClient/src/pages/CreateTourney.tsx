import { useEffect, useState, type ChangeEvent } from "react";

import BasicHeading from "@/components/HeadingOne";
import BasicInput from "@/components/BasicInput";
import SubmitButton from "@/components/SubmitButton";
import { RequestService } from "@/services/RequestService";
import { validateGameIdResponse, validateTotalPlayers } from "@/services/validationService";
import { INVALID_CHARACTERS, MAX_SUPPORTED_PLAYERS, SERVER_ERROR, SUBMIT_SUCCESS } from "@/constants/AppConstants";
import { useNavigate } from 'react-router';
import { useGameData } from "@/hooks/useGameData";
import HeadingTwo from "@/components/HeadingTwo";
import { Character } from "@/models/entities/Character";
import { CreateGameWithModeRequest, CreateGameWithModeResponse } from "@/models/entities/Bracket";
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { TierPlacement } from "@/models/Enums/TierPlacement";
import { WeightClass } from "@/models/Enums/WeightClass";
import { v4 as uuidv4 } from "uuid";
import PersistentConnection from "@/services/PersistentConnection";
import
{
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

/*Ready for E2E testing */

const CreateTourney = () =>
{
  type SessionStatusResponse = {
    UserId?: string;
    UserName?: string;
  };

  type AddPlayerPayload = {
    Id: string;
    userId: string;
    displayName: string;
    currentScore: number;
    currentRound: number;
    currentCharacter: {
      id: string;
      characterName: keyof typeof CharacterName;
      archetype: keyof typeof Archetype;
      fallSpeed: keyof typeof FallSpeed;
      tierPlacement: keyof typeof TierPlacement;
      weightClass: keyof typeof WeightClass;
    };
    currentGameId: string;
  };

  const getEnumKeyByValue = <TMap extends Record<string, string>>(
    enumMap: TMap,
    value: string
  ): keyof TMap | null =>
  {
    const foundPair = Object.entries(enumMap).find(([, enumValue]) => enumValue === value);
    return (foundPair?.[0] as keyof TMap) ?? null;
  };

  //dynamic import react router useNavigate
  const navigate = useNavigate();

  //setup store for gameId after creation
  const { setGameId: setId, setPlayerId: setPlayerId } = useGameData();

  //variables for users selections
  //will set this with dropdown component with preset values
  const [numPlayers, setNumPlayers] = useState("");

  //gameType true is double elimination
  //gameType false is single elimination
  //will set this with an enum like object
  const [gameType, setGameType] = useState(false);
  const [characters, setCharacters] = useState<Character[]>([]);
  const [currentCharacter, setCurrentCharacter] = useState({} as Character);

  useEffect(() =>
  {
    const characterModuleMap = import.meta.glob("../models/entities/Characters/*.ts");

    const fetchAllCharacters = async () =>
    {
      const modulePromises = Object.values(characterModuleMap).map((dynamicImport) => dynamicImport());
      const resolvedModules = await Promise.all(modulePromises);
      const characterObjects = resolvedModules.map((module) => (module as { default: Character }).default);
      setCharacters(characterObjects);
    };

    fetchAllCharacters();
  }, []);

  //handle max player selection
  const handleMaxPlayers = (e: ChangeEvent<HTMLInputElement>) =>
  {
    const numplayers = parseInt(e.target.value);
    if (validateTotalPlayers(numplayers))
    {
      setNumPlayers(e.target.value);

    }
    else
    {
      window.alert(INVALID_CHARACTERS("Number of Players"));
    }

  }

  //handle select game selection
  const handleSelectGameType = async (e: ChangeEvent<HTMLSelectElement>) =>
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
    const mappedCharacterName = getEnumKeyByValue(CharacterName, currentCharacter.characterName);
    const mappedArchetype = getEnumKeyByValue(Archetype, currentCharacter.archetype);
    const mappedFallSpeed = getEnumKeyByValue(FallSpeed, currentCharacter.fallSpeed);
    const mappedTierPlacement = getEnumKeyByValue(TierPlacement, currentCharacter.tierPlacement);
    const mappedWeightClass = getEnumKeyByValue(WeightClass, currentCharacter.weightClass);

    if (!mappedCharacterName || !mappedArchetype || !mappedFallSpeed || !mappedTierPlacement || !mappedWeightClass)
    {
      window.alert(INVALID_CHARACTERS("Character Selection"));
      return;
    }

    try
    {
      //call request service and provide no body object since our api does not need a body for createGame
      const selectedMode = gameType ? "DOUBLE_ELIMINATION" : "SINGLE_ELIMINATION";
      const response = await RequestService<"createGameWithMode", CreateGameWithModeRequest, CreateGameWithModeResponse>(
        "createGameWithMode",
        {
          body: {
            bracketMode: selectedMode
          }
        }
      );
      const gameId = response?.gameId ?? response?.GameId;

      if (validateGameIdResponse(gameId ?? ""))
      {
        let hostUserId = "";
        let hostDisplayName = "";

        try
        {
          const session = await RequestService<"sessionStatus", never, SessionStatusResponse>("sessionStatus");
          hostUserId = session?.UserId?.trim() ?? "";
          hostDisplayName = session?.UserName?.trim() ?? "";
        }
        catch (error)
        {
          console.error("Failed to resolve session details before AddPlayer", error);
        }

        if (!hostDisplayName)
        {
          hostDisplayName = "Host";
        }

        if (!hostUserId)
        {
          hostUserId = "host-user";
        }

        const hostPlayerId = uuidv4();
        const hostPlayerPayload: AddPlayerPayload = {
          Id: hostPlayerId,
          userId: hostUserId,
          displayName: hostDisplayName,
          currentScore: 0,
          currentRound: 0,
          currentCharacter: {
            id: currentCharacter.id,
            characterName: mappedCharacterName,
            archetype: mappedArchetype,
            fallSpeed: mappedFallSpeed,
            tierPlacement: mappedTierPlacement,
            weightClass: mappedWeightClass,
          },
          currentGameId: gameId!,
        };

        await RequestService("addPlayers", {
          body: hostPlayerPayload,
          routeParams: {
            gameId: gameId!
          }
        });

        const lobbyConnection = new PersistentConnection();
        try
        {
          await lobbyConnection.createPlayerConnection(gameId!);
          await lobbyConnection.updateOthers(gameId!);
        }
        catch (error)
        {
          console.error("Failed to notify lobby updates", error);
        }
        finally
        {
          await lobbyConnection.disconnect();
        }

        //use setId useContext function
        setId(gameId!);
        setPlayerId(hostPlayerId);

        window.alert(SUBMIT_SUCCESS("Create Tourney"));
        //force navigation without user intervention upon request completion and alert dismissal
        navigate("/lobby");
      }
      else
      {
        window.alert(SERVER_ERROR("Create Tourney"));
      }
    }
    catch (error)
    {
      console.error("Create Tourney submit failed", error);
      window.alert(SERVER_ERROR("Create Tourney"));
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
          <HeadingTwo headingText={`Mode: ${gameType ? "Double Elimination" : "Single Elimination"}`} />
          <HeadingTwo headingText={`Enter Total Players (Up to ${MAX_SUPPORTED_PLAYERS})`} />
          <BasicInput labelText="" htmlFor="maxPlayers" name="maxPlayers" id="maxPlayers" value={numPlayers} onChange={handleMaxPlayers} />
          <DropdownMenu>
            <DropdownMenuTrigger className="shrink p-2 m-2 bg-white hover:ring-2 hover:ring-green-400 text-black  font-bold rounded shadow-md 
      transition duration-300 ease-in-out focus:outline-none focus:ring-2 focus:ring-green-400 focus:ring-opacity-75">{currentCharacter.characterName || "Choose Your Fighter"}</DropdownMenuTrigger>
            <DropdownMenuContent>
              {characters.map((character, index) => (
                <DropdownMenuItem key={index}
                  onSelect={() => setCurrentCharacter(character)}>
                  {character.characterName}
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
          <SubmitButton buttonLabel="Create Tourney" onSubmit={handleSubmit} />
        </div>
      </div>
    </div>
  );
}

export default CreateTourney;
