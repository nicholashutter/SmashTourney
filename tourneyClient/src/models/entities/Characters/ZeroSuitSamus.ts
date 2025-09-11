import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const ZeroSuitSamus: Character =
{
    id: CharacterId.ZeroSuitSamus,
    characterName: CharacterName.ZERO_SUIT_SAMUS,
    archetype: Archetype.NINJA,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.FEATHERWEIGHT,
    tierPlacement: TierPlacement.A
}

export default ZeroSuitSamus; 