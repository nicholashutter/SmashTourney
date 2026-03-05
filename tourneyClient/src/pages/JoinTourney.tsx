import { useState, useEffect, type ChangeEvent } from "react";
import { useNavigate } from "react-router";
import { RequestService } from "@/services/RequestService";
import { useGameData } from "@/hooks/useGameData";
import { validateInput } from "@/services/validationService";
import { INVALID_CHARACTERS } from "@/constants/AppConstants";
import { Character } from "@/models/entities/Character";
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
import { isValidGuid, normalizeGameId, resolveCharacterMappings } from "@/services/playerSetupService";

// Renders player join flow for an existing tournament lobby.

const JoinTourney = () =>
{
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


  // Handles route navigation after join flow events.
  const navigate = useNavigate();

  const { setGameId, gameId, setPlayerId, playerId, setIsHost } = useGameData();
  // Stores joining player's display name.
  const [displayName, setDisplayName] = useState("");

  // Stores characters available in the selection dropdown.
  const [characters, setCharacters] = useState<Character[]>([]);

  // Stores the joining player's selected character.
  const [currentCharacter, setCurrentCharacter] = useState({} as Character);
  const [joinStatus, setJoinStatus] = useState("Idle");
  const [isJoining, setIsJoining] = useState(false);



  // Loads all selectable character definitions for join flow setup.
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

  // Stores validated session code input.
  const gameIdHandler = (e: ChangeEvent<HTMLInputElement>) =>
  {
    const normalizedGameId = normalizeGameId(e.target.value);
    const validateGameId = validateInput(normalizedGameId);
    if (validateGameId.isValid)
    {
      setGameId(normalizedGameId);
    }
    else
    {
      window.alert(INVALID_CHARACTERS("GameId"));
    }

  }

  // Stores validated display name input.
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

  // Submits join request, connects to lobby, and routes to lobby page.
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

    const mappedCharacter = resolveCharacterMappings(currentCharacter, {
      CharacterName,
      Archetype,
      FallSpeed,
      TierPlacement,
      WeightClass,
    });

    if (!mappedCharacter)
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
        characterName: mappedCharacter.characterName as keyof typeof CharacterName,
        archetype: mappedCharacter.archetype as keyof typeof Archetype,
        fallSpeed: mappedCharacter.fallSpeed as keyof typeof FallSpeed,
        tierPlacement: mappedCharacter.tierPlacement as keyof typeof TierPlacement,
        weightClass: mappedCharacter.weightClass as keyof typeof WeightClass,
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
      setIsHost(false);
      window.alert("You joined the tournament successfully. You are now moving to the lobby.");
      navigate("/lobby");
    }
    catch (err)
    {
      console.error(err);
      setJoinStatus("Join failed. Please try again.");
      window.alert("We could not join that tournament. You will stay on this page so you can fix details and try again.");
    }
    finally
    {
      setIsJoining(false);
      await lobbyConnection.disconnect();
    }

  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
      <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">
        <title>Join Tourney</title>
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Join Room" headingColors="white" />
          <BasicInput labelText="Session Code:" htmlFor="sessionCode"
            id="gameId" name="gameId" value={gameId ?? ""} onChange={gameIdHandler} />
          <BasicInput labelText="Enter Player Name:" htmlFor="playerName"
            id="displayName" name="displayName" value={displayName} onChange={displayNameHandler} />
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
          <HeadingTwo headingText={joinStatus} />
          <SubmitButton buttonLabel={isJoining ? "Joining..." : "Join Room"} onSubmit={submitHandler} />
          <BasicButton buttonLabel="Return to Main Menu" href="/" />

        </div>
      </div>
    </div >
  );
};

export default JoinTourney;
