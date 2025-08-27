import { FallSpeed, FallSpeedType } from "../../utilities/Enums/FallSpeed";
import { Archetype, ArchetypeType } from "../../utilities/Enums/Archetype";
import { CharacterName, CharacterNameType } from "../../utilities/Enums/CharacterName";
import { TierPlacement, TierPlacementType } from "../../utilities/Enums/TierPlacement";
import { WeightClass, WeightClassType } from "../../utilities/Enums/WeightClass";

export interface Character
{
    characterName: CharacterNameType;
    archetype: ArchetypeType;
    fallSpeed: FallSpeedType;
    tierPlacement: TierPlacementType;
    weightClass: WeightClassType;
}