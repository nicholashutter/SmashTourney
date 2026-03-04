import { Character } from "@/models/entities/Character";
import { SessionIdentity, SessionStatusResponse } from "@/models/entities/SessionIdentity";
import { RequestService } from "@/services/RequestService";

// Resolves enum key by matching its string value.
export const getEnumKeyByValue = <TMap extends Record<string, string>>(
    enumMap: TMap,
    value: string
): keyof TMap | null =>
{
    for (const [enumKey, enumValue] of Object.entries(enumMap))
    {
        if (enumValue === value)
        {
            return enumKey as keyof TMap;
        }
    }

    return null;
};

// Normalizes a session code by trimming spaces.
export const normalizeGameId = (value: string): string => value.replace(/\s+/g, "").trim();

// Validates whether a string is a GUID.
export const isValidGuid = (value: string): boolean =>
    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/.test(value);

// Converts selected character values into backend enum key payload values.
export const resolveCharacterMappings = (
    currentCharacter: Character,
    enumMaps: {
        CharacterName: Record<string, string>;
        Archetype: Record<string, string>;
        FallSpeed: Record<string, string>;
        TierPlacement: Record<string, string>;
        WeightClass: Record<string, string>;
    }
):
    {
        characterName: string;
        archetype: string;
        fallSpeed: string;
        tierPlacement: string;
        weightClass: string;
    } | null =>
{
    if (!currentCharacter)
    {
        return null;
    }

    const mappedCharacterName = getEnumKeyByValue(enumMaps.CharacterName, currentCharacter.characterName);
    const mappedArchetype = getEnumKeyByValue(enumMaps.Archetype, currentCharacter.archetype);
    const mappedFallSpeed = getEnumKeyByValue(enumMaps.FallSpeed, currentCharacter.fallSpeed);
    const mappedTierPlacement = getEnumKeyByValue(enumMaps.TierPlacement, currentCharacter.tierPlacement);
    const mappedWeightClass = getEnumKeyByValue(enumMaps.WeightClass, currentCharacter.weightClass);

    if (!mappedCharacterName || !mappedArchetype || !mappedFallSpeed || !mappedTierPlacement || !mappedWeightClass)
    {
        return null;
    }

    return {
        characterName: mappedCharacterName,
        archetype: mappedArchetype,
        fallSpeed: mappedFallSpeed,
        tierPlacement: mappedTierPlacement,
        weightClass: mappedWeightClass,
    };
};

// Fetches and normalizes session identity values from the backend.
export const getSessionIdentity = async (): Promise<SessionIdentity> =>
{
    const session = await RequestService<"sessionStatus", never, SessionStatusResponse>("sessionStatus");

    let userIdValue = "";
    if (session && typeof session.UserId === "string")
    {
        userIdValue = session.UserId;
    }
    else if (session && typeof session.userId === "string")
    {
        userIdValue = session.userId;
    }

    return {
        userId: userIdValue.trim(),
    };
};
