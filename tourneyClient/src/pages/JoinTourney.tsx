import { useState, useEffect, type ChangeEvent } from "react";
import { useNavigate, useSearchParams } from "react-router";
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
import { v4 as uuidv4 } from "uuid";
import { PersistentConnection } from "@/services/PersistentConnection"
import BasicInput from "@/components/BasicInput";
import BasicHeading from "@/components/HeadingOne";
import SubmitButton from "@/components/SubmitButton";
import BasicButton from "@/components/BasicButton";
import FighterSelect from "@/components/FighterSelect";
import { isValidGuid, normalizeGameId, resolveCharacterMappings } from "@/services/playerSetupService";
import { AddPlayerPayload } from "@/models/types/playerPayload";
import { loadCharacterCatalog } from "@/lib/loadCharacterCatalog";
import PageShell from "@/components/PageShell";
import StatusBanner, { StatusMessage } from "@/components/StatusBanner";
import { lobbyPath } from "@/services/gameRoutes";

// Renders player join flow for an existing tournament lobby.

const JoinTourney = () =>
{
  // Handles route navigation after join flow events.
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const { setGameId, gameId, setPlayerId, playerId, setIsHost } = useGameData();
  // Stores joining player's display name.
  const [displayName, setDisplayName] = useState("");

  // Stores characters available in the selection dropdown.
  const [characters, setCharacters] = useState<Character[]>([]);

  // Stores the joining player's selected character.
  const [currentCharacter, setCurrentCharacter] = useState({} as Character);
  const [status, setStatus] = useState<StatusMessage | null>(null);
  const [isJoining, setIsJoining] = useState(false);



  // Loads all selectable character definitions for join flow setup.
  useEffect(() =>
  {
    const fetchAllCharacters = async () =>
    {
      const characterCatalog = await loadCharacterCatalog();
      setCharacters(characterCatalog);
    };

    fetchAllCharacters();
  }, []);

  // Prefills the session code when arriving from a game link.
  //
  // Someone who opens a shared URL for a tournament they have not joined is
  // sent here, and they already told us which game they meant by clicking the
  // link. Making them retype a GUID from a phone would be the wrong ending to
  // that story.
  useEffect(() =>
  {
    const gameIdFromLink = searchParams.get("gameId");
    if (gameIdFromLink)
    {
      setGameId(normalizeGameId(gameIdFromLink));
    }
  }, [searchParams, setGameId]);

  // Stores validated session code input.
  const handleGameIdChange = (e: ChangeEvent<HTMLInputElement>) =>
  {
    const normalizedGameId = normalizeGameId(e.target.value);
    const validateGameId = validateInput(normalizedGameId);
    if (validateGameId.isValid)
    {
      setGameId(normalizedGameId);
    }
    else
    {
      setStatus({ text: INVALID_CHARACTERS("GameId"), tone: "error" });
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
      setStatus({ text: INVALID_CHARACTERS("Display Name"), tone: "error" });
    }

  }

  // Submits join request, connects to lobby, and routes to lobby page.
  const handleSubmit = async () =>
  {
    if (isJoining)
    {
      return;
    }

    const normalizedGameId = normalizeGameId(gameId ?? "");

    if (!normalizedGameId)
    {
      setStatus({ text: INVALID_CHARACTERS("GameId"), tone: "error" });
      return;
    }

    if (!isValidGuid(normalizedGameId))
    {
      setStatus({ text: "Session code must be a valid GUID.", tone: "error" });
      return;
    }

    if (!displayName.trim())
    {
      setStatus({ text: INVALID_CHARACTERS("Display Name"), tone: "error" });
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
      setStatus({ text: INVALID_CHARACTERS("Character Selection"), tone: "error" });
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
      setStatus({ text: "Joining room...", tone: "info" });

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

      setStatus({ text: "Join successful. Opening lobby...", tone: "success" });
      setIsHost(false);
      navigate(lobbyPath(normalizedGameId));
    }
    catch (err)
    {
      console.error(err);
      setStatus({ text: "Join failed. Check your details and try again.", tone: "error" });
    }
    finally
    {
      setIsJoining(false);
      await lobbyConnection.disconnect();
    }

  }

  return (
    <PageShell pageTitle="Join Tourney">
        <div className='shrink flex flex-col text-2xl p-4 m-4 '>
          <BasicHeading headingText="Join Room" headingColors="white" />
          <BasicInput labelText="Session Code:" htmlFor="sessionCode"
            id="gameId" name="gameId" value={gameId ?? ""} onChange={handleGameIdChange} />
          <BasicInput labelText="Enter Player Name:" htmlFor="playerName"
            id="displayName" name="displayName" value={displayName} onChange={handleDisplayNameChange} />
          <FighterSelect
            characters={characters}
            selectedCharacter={currentCharacter.id ? currentCharacter : null}
            onSelect={setCurrentCharacter}
          />
          <StatusBanner status={status} />
          <SubmitButton buttonLabel={isJoining ? "Joining..." : "Join Room"} onSubmit={handleSubmit} />
          <BasicButton buttonLabel="Return to Main Menu" href="/" />

        </div>
    </PageShell>
  );
};

export { JoinTourney };
