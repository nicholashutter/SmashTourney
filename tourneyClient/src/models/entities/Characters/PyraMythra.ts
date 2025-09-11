import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const PyraMythra: Character =
{
    id: CharacterId.Pyra,
    characterName: CharacterName.PYRA_AND_MYTHRA,
    archetype: Archetype.DYNAMIC,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.LIGHTWEIGHT,
    tierPlacement: TierPlacement.S
}

export default PyraMythra; 