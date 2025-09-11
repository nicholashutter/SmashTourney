import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Ryu: Character =
{
    id: CharacterId.Ryu,
    characterName: CharacterName.RYU,
    archetype: Archetype.BAIT_AND_PUNISH,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.MIDDLEWEIGHT,
    tierPlacement: TierPlacement.B
}

export default Ryu; 