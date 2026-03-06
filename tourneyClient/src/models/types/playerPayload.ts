import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { TierPlacement } from "@/models/Enums/TierPlacement";
import { WeightClass } from "@/models/Enums/WeightClass";

export type BackendCharacterPayload = {
    id: string;
    characterName: keyof typeof CharacterName;
    archetype: keyof typeof Archetype;
    fallSpeed: keyof typeof FallSpeed;
    tierPlacement: keyof typeof TierPlacement;
    weightClass: keyof typeof WeightClass;
};

export type AddPlayerPayload = {
    Id: string;
    displayName: string;
    currentScore: number;
    currentRound: number;
    currentCharacter: BackendCharacterPayload;
    currentGameId: string;
};