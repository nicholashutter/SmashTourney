import { CharacterName } from "@/models/Enums/CharacterName";
import { Character } from "../Character"
import { CharacterId } from "@/models/Enums/CharacterId";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { TierPlacement } from "@/models/Enums/TierPlacement";
import { WeightClass } from "@/models/Enums/WeightClass";

export const DrMario: Character =
{
    id: CharacterId.DrMario,
    characterName: CharacterName.DR_MARIO,
    archetype: Archetype.ALL_ROUNDER,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.LIGHTWEIGHT,
    tierPlacement: TierPlacement.D

}

export default DrMario; 