import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Ridley: Character =
{
    id: CharacterId.Ridley,
    characterName: CharacterName.RIDLEY,
    archetype: Archetype.PRECISION,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.MIDDLEWEIGHT,
    tierPlacement: TierPlacement.C
}

export default Ridley; 