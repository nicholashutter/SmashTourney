import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const KingDedede: Character =
{
    id: CharacterId.KingDedede,
    characterName: CharacterName.KING_DEDEDE,
    archetype: Archetype.TANK,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.HEAVYWEIGHT,
    tierPlacement: TierPlacement.D
}

export default KingDedede; 