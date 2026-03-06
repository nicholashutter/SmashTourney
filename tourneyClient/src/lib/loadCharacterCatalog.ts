import { Character } from "@/models/entities/Character";

const isCharacter = (value: unknown): value is Character =>
{
    if (!value || typeof value !== "object")
    {
        return false;
    }

    const candidate = value as Character;
    return Boolean(
        candidate.id &&
        candidate.characterName &&
        candidate.archetype &&
        candidate.fallSpeed &&
        candidate.tierPlacement &&
        candidate.weightClass
    );
};

// Loads all selectable characters from model modules using named exports.
export const loadCharacterCatalog = async (): Promise<Character[]> =>
{
    const characterModuleMap = import.meta.glob("../models/entities/Characters/*.ts");
    const modulePromises = Object.values(characterModuleMap).map((dynamicImport) => dynamicImport());
    const resolvedModules = await Promise.all(modulePromises);

    return resolvedModules
        .map((module) => Object.values(module as Record<string, unknown>).find(isCharacter))
        .filter((character): character is Character => Boolean(character));
};