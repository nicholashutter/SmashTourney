import { CharacterName } from "@/models/Enums/CharacterName";
import { Character } from "../Character"
import { CharacterId } from "@/models/Enums/CharacterId";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Robin: Character =
{
    id: CharacterId.Robin,
    characterName: CharacterName.ROBIN,
    archetype: Archetype.ZONER,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.LIGHTWEIGHT,
    tierPlacement: TierPlacement.C
}

export default Robin; 