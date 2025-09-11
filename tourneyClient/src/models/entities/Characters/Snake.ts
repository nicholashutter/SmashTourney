import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Snake: Character =
{
    id: CharacterId.Snake,
    characterName: CharacterName.SNAKE,
    archetype: Archetype.TRAPPER,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.MIDDLEWEIGHT,
    tierPlacement: TierPlacement.S
}

export default Snake; 