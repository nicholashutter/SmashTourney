import { useState, useEffect, type ChangeEvent } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import { useGameData } from "@/hooks/useGameData";
import { validateInput } from "@/services/validationService";
import { INVALID_CHARACTERS, SERVER_ERROR, SUBMIT_SUCCESS } from "@/constants/AppConstants";
import { Character } from "@/models/entities/Character.ts";
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { TierPlacement } from "@/models/Enums/TierPlacement";
import { WeightClass } from "@/models/Enums/WeightClass";
import { Player } from "@/models/entities/Player";
import { v4 as uuidv4 } from "uuid";
import PersistentConnection from "@/services/PersistentConnection"
import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import HeadingTwo from "@/components/HeadingTwo";
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
  type SessionStatusResponse = {
    userName?: string;
    UserName?: string;
  };

  type BackendCharacterPayload = {
    id: string;
    characterName: keyof typeof CharacterName;
    archetype: keyof typeof Archetype;
    fallSpeed: keyof typeof FallSpeed;
    tierPlacement: keyof typeof TierPlacement;
    weightClass: keyof typeof WeightClass;
  };

  type AddPlayerPayload = Omit<Player, "currentCharacter"> & {
    currentCharacter: BackendCharacterPayload;
  };

  const getEnumKeyByValue = <TMap extends Record<string, string>>(
    enumMap: TMap,
    value: string
  ): keyof TMap | null =>
  {
    const foundPair = Object.entries(enumMap).find(([, enumValue]) => enumValue === value);
    return (foundPair?.[0] as keyof TMap) ?? null;
  };

  const normalizeGameId = (value: string): string => value.replace(/\s+/g, "").trim();

  const isValidGuid = (value: string): boolean =>
    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/.test(value);


  //from react router for navigation without reloading
  const navigate = useNavigate();

  const { setGameId: setGameId, gameId: gameId, setPlayerId: setPlayerId, playerId: playerId } = useGameData();
  //player should enter
  const [displayName, setDisplayName] = useState("");

  //array will be loaded with characters using dynamic import.meta.glob provided by vite
  const [characters, setCharacters] = useState<Character[]>([]);

  //player will choose through dropdown components
  const [currentCharacter, setCurrentCharacter] = useState({} as Character);
  const [joinStatus, setJoinStatus] = useState("Idle");
  const [isJoining, setIsJoining] = useState(false);



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
      const characterObjects = resolvedModules.map((module) => (module as { default: Character }).default);

      //save the array of character objects to state
      setCharacters(characterObjects);
    };

    //Run the async loader function
    fetchAllCharacters();
  }, []);

  useEffect(() =>
  {
    let mounted = true;

    const preloadDisplayName = async () =>
    {
      try
      {
        const session = await RequestService<"sessionStatus", never, SessionStatusResponse>("sessionStatus");
        const sessionUserName = (session?.UserName ?? session?.userName ?? "").trim();

        if (mounted && sessionUserName)
        {
          setDisplayName((existing) => existing || sessionUserName);
        }
      }
      catch (error)
      {
        console.error("Unable to preload player name from session", error);
      }
    };

    preloadDisplayName();

    return () =>
    {
      mounted = false;
    };
  }, []);

  const gameIdHandler = (e: ChangeEvent<HTMLInputElement>) =>
  {
    const normalizedGameId = normalizeGameId(e.target.value);
    const validateGameId = validateInput(normalizedGameId);
    if (validateGameId.isValid)
    {
      //custom useContext for gameId
      setGameId(normalizedGameId);
    }
    else
    {
      window.alert(INVALID_CHARACTERS("GameId"));
    }

  }

  const displayNameHandler = (e: ChangeEvent<HTMLInputElement>) =>
  {
    const validateDisplayName = validateInput(e.target.value);
    if (validateDisplayName.isValid)
    {
      setDisplayName(e.target.value);
    }
    else
    {
      window.alert(INVALID_CHARACTERS("Display Name"));
    }

  }

  const submitHandler = async () =>
  {
    if (isJoining)
    {
      return;
    }

    const normalizedGameId = normalizeGameId(gameId ?? "");

    if (!normalizedGameId)
    {
      window.alert(INVALID_CHARACTERS("GameId"));
      return;
    }

    if (!isValidGuid(normalizedGameId))
    {
      window.alert("Session code must be a valid GUID.");
      return;
    }

    if (!displayName.trim())
    {
      window.alert(INVALID_CHARACTERS("Display Name"));
      return;
    }

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

    const resolvedPlayerId = playerId ?? uuidv4();
    if (!playerId)
    {
      setPlayerId(resolvedPlayerId);
    }

    const playerPayload: AddPlayerPayload =
    {
      Id: resolvedPlayerId,
      displayName: displayName.trim(),
      currentScore: 0,
      currentRound: 0,
      currentCharacter:
      {
        id: currentCharacter.id,
        characterName: mappedCharacterName,
        archetype: mappedArchetype,
        fallSpeed: mappedFallSpeed,
        tierPlacement: mappedTierPlacement,
        weightClass: mappedWeightClass,
      },
      currentGameId: normalizedGameId
    };

    const lobbyConnection = new PersistentConnection();

    try
    {
      setIsJoining(true);
      setJoinStatus("Joining room...");

      await RequestService(
        "addPlayers",
        {
          body: playerPayload,
          routeParams:
          {
            gameId: normalizedGameId
          }
        }
      );

      await lobbyConnection.createPlayerConnection(normalizedGameId);
      await lobbyConnection.updateOthers(normalizedGameId);

      setJoinStatus("Join successful. Opening lobby...");
      window.alert(SUBMIT_SUCCESS("Join Tourney"));
      navigate("/lobby");
    }
    catch (err)
    {
      console.error(err);
      setJoinStatus("Join failed. Please try again.");
      window.alert(SERVER_ERROR("Join Tourney"));
    }
    finally
    {
      setIsJoining(false);
      await lobbyConnection.disconnect();
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
          <HeadingTwo headingText={joinStatus} />
          <SubmitButton buttonLabel={isJoining ? "Joining..." : "Join Room"} onSubmit={submitHandler} />
          <BasicButton buttonLabel="Return to Main Menu" href="/" />

        </div>
      </div>
    </div >
  );
};

export default JoinTourney;
