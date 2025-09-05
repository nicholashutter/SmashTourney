
import { FallSpeed, FallSpeedType } from "../Enums/FallSpeed"
import { Archetype, ArchetypeType } from "../Enums/Archetype";
import { CharacterName, CharacterNameType } from "../Enums/CharacterName";
import { TierPlacement, TierPlacementType } from "../Enums/TierPlacement";
import { WeightClass, WeightClassType } from "../Enums/WeightClass";

export interface Character
{
    characterName: CharacterNameType;
    archetype: ArchetypeType;
    fallSpeed: FallSpeedType;
    tierPlacement: TierPlacementType;
    weightClass: WeightClassType;
}