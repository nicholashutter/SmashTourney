import { useEffect, useState, type ChangeEvent } from "react";

import BasicHeading from "@/components/HeadingOne";
import BasicInput from "@/components/BasicInput";
import SubmitButton from "@/components/SubmitButton";
import { RequestService } from "@/services/RequestService";
import { validateGameIdResponse, validateInput, validateTotalPlayers } from "@/services/validationService";
import { INVALID_CHARACTERS, MAX_SUPPORTED_PLAYERS } from "@/constants/AppConstants";
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
import { PersistentConnection } from "@/services/PersistentConnection";
import { getSessionIdentity, resolveCharacterMappings } from "@/services/playerSetupService";
import { AddPlayerPayload } from "@/models/types/playerPayload";
import { loadCharacterCatalog } from "@/lib/loadCharacterCatalog";
import
{
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

// Renders host setup flow to create a tournament and join its lobby.
const CreateTourney = () =>
{
  // Handles route navigation after create flow events.
  const navigate = useNavigate();

  // Stores created session identity for later pages.
  const { setGameId: setId, setPlayerId: setPlayerId, setIsHost } = useGameData();

  // Stores host-selected maximum player count.
  const [numPlayers, setNumPlayers] = useState("");

  // Stores whether selected bracket mode is double elimination.
  const [gameType, setGameType] = useState(false);
  const [displayName, setDisplayName] = useState("");
  const [characters, setCharacters] = useState<Character[]>([]);
  const [currentCharacter, setCurrentCharacter] = useState({} as Character);

  // Loads all selectable character definitions for host setup.
  useEffect(() =>
  {
    const fetchAllCharacters = async () =>
    {
      const characterCatalog = await loadCharacterCatalog();
      setCharacters(characterCatalog);
    };

    fetchAllCharacters();
  }, []);

  // Stores validated maximum player count input.
  const handleMaxPlayers = (e: ChangeEvent<HTMLInputElement>) =>
  {
    const parsedPlayers = parseInt(e.target.value, 10);
    if (validateTotalPlayers(parsedPlayers))
    {
      setNumPlayers(e.target.value);

    }
    else
    {
      window.alert(INVALID_CHARACTERS("Number of Players"));
    }

  }

  // Stores validated display name input.
  const handleDisplayNameChange = (e: ChangeEvent<HTMLInputElement>) =>
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

  // Sets selected tournament mode from the ruleset dropdown.
  const handleSelectGameType = (e: ChangeEvent<HTMLSelectElement>) =>
  {
    const selected = e.target.value;

    switch (selected)
    {
      case "Single Elimination":
        setGameType(false);
        break;
      case "Double Elimination":
        setGameType(true);
        break
      default:
        console.log("Invalid game type selection");
        break;
    }
  }

  // Creates the game, adds the host player, and opens the lobby.
  const handleSubmit = async () =>
  {
    const requestedTotalPlayers = Number.parseInt(numPlayers, 10);
    if (!validateTotalPlayers(requestedTotalPlayers))
    {
      window.alert(INVALID_CHARACTERS("Number of Players"));
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

    try
    {
      let selectedMode: "DOUBLE_ELIMINATION" | "SINGLE_ELIMINATION" = "SINGLE_ELIMINATION";
      if (gameType)
      {
        selectedMode = "DOUBLE_ELIMINATION";
      }

      const response = await RequestService<"createGameWithMode", CreateGameWithModeRequest, CreateGameWithModeResponse>(
        "createGameWithMode",
        {
          body: {
            bracketMode: selectedMode,
            totalPlayers: requestedTotalPlayers
          }
        }
      );
      let resolvedGameId = "";
      if (response && response.gameId)
      {
        resolvedGameId = response.gameId;
      }
      else if (response && response.GameId)
      {
        resolvedGameId = response.GameId;
      }

      if (validateGameIdResponse(resolvedGameId))
      {
        let hostUserId = "";

        try
        {
          const sessionIdentity = await getSessionIdentity();
          hostUserId = sessionIdentity.userId;
        }
        catch (error)
        {
          console.error("Failed to resolve session details before AddPlayer", error);
        }

        if (!hostUserId)
        {
          window.alert("Your session appears to have expired. Please log in again.");
          navigate("/");
          return;
        }

        const hostDisplayName = displayName.trim();

        if (!hostDisplayName)
        {
          window.alert(INVALID_CHARACTERS("Display Name"));
          return;
        }

        const hostPlayerId = uuidv4();
        const hostPlayerPayload: AddPlayerPayload = {
          Id: hostPlayerId,
          displayName: hostDisplayName,
          currentScore: 0,
          currentRound: 0,
          currentCharacter: {
            id: uuidv4(),
            characterName: mappedCharacter.characterName as keyof typeof CharacterName,
            archetype: mappedCharacter.archetype as keyof typeof Archetype,
            fallSpeed: mappedCharacter.fallSpeed as keyof typeof FallSpeed,
            tierPlacement: mappedCharacter.tierPlacement as keyof typeof TierPlacement,
            weightClass: mappedCharacter.weightClass as keyof typeof WeightClass,
          },
          currentGameId: resolvedGameId,
        };

        await RequestService("addPlayers", {
          body: hostPlayerPayload,
          routeParams: {
            gameId: resolvedGameId
          }
        });

        const lobbyConnection = new PersistentConnection();
        try
        {
          await lobbyConnection.createPlayerConnection(resolvedGameId);
          await lobbyConnection.updateOthers(resolvedGameId);
        }
        catch (error)
        {
          console.error("Failed to notify lobby updates", error);
        }
        finally
        {
          await lobbyConnection.disconnect();
        }

        setId(resolvedGameId);
        setPlayerId(hostPlayerId);
        setIsHost(true);

        window.alert("Tournament created successfully. You are now moving to the lobby as the host.");
        navigate("/lobby");
      }
      else
      {
        window.alert("We could not create the tournament. You will stay on the create page so you can try again.");
      }
    }
    catch (error)
    {
      console.error("Create Tourney submit failed", error);
      window.alert("We could not create the tournament. You will stay on the create page so you can try again.");
    }

  }

  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw">
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
          <BasicInput labelText="Enter Player Name:" htmlFor="playerName" id="displayName" name="displayName" value={displayName} onChange={handleDisplayNameChange} />
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

export { CreateTourney };
