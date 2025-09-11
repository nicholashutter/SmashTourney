import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Yoshi: Character =
{
    id: CharacterId.Yoshi,
    characterName: CharacterName.YOSHI,
    archetype: Archetype.ALL_ROUNDER,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.MIDDLEWEIGHT,
    tierPlacement: TierPlacement.S
}

export default Yoshi; 