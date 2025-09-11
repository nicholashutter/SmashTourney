import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const KingKRool: Character =
{
    id: CharacterId.KingKRool,
    characterName: CharacterName.KING_K_ROOL,
    archetype: Archetype.BAIT_AND_PUNISH,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.HEAVYWEIGHT,
    tierPlacement: TierPlacement.D
}

export default KingKRool; 