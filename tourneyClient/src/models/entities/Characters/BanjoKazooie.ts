import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const BanjoKazooie: Character =
{
    id: CharacterId.BanjoKazooie,
    characterName: CharacterName.BANJO_AND_KAZOOIE,
    archetype: Archetype.TURTLE,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.HEAVYWEIGHT,
    tierPlacement: TierPlacement.C
}

export default BanjoKazooie; 