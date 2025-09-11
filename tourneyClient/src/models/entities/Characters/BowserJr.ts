import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const BowserJr: Character =
{
    id: CharacterId.BowserJr,
    characterName: CharacterName.BOWSER_JR,
    archetype: Archetype.BAIT_AND_PUNISH,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.MIDDLEWEIGHT,
    tierPlacement: TierPlacement.C
}

export default BowserJr; 