import { FallSpeed, FallSpeedType } from "../../utilities/FallSpeed";
import { Archetype, ArchetypeType } from "../../utilities/Archetype";
import { CharacterName, CharacterNameType } from "../../utilities/CharacterName";
import { TierPlacement, TierPlacementType } from "../../utilities/TierPlacement";
import { WeightClass, WeightClassType } from "../../utilities/WeightClass";

export interface Character
{
    characterName: CharacterNameType;
    archetype: ArchetypeType;
    fallSpeed: FallSpeedType;
    tierPlacement: TierPlacementType;
    weightClass: WeightClassType;
}