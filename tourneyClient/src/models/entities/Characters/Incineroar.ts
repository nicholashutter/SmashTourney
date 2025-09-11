import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Incineroar: Character =
{
    id: CharacterId.Incineroar,
    characterName: CharacterName.INCINEROAR,
    archetype: Archetype.TANK,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.HEAVYWEIGHT,
    tierPlacement: TierPlacement.C
}

export default Incineroar; 