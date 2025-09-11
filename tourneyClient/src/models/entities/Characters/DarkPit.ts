import { CharacterName } from "@/models/Enums/CharacterName";
import { Character } from "../Character"
import { CharacterId } from "@/models/Enums/CharacterId";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const DarkPit: Character =
{
    id: CharacterId.DarkPit,
    characterName: CharacterName.DARK_PIT,
    archetype: Archetype.ALL_ROUNDER,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.LIGHTWEIGHT,
    tierPlacement: TierPlacement.B
}

export default DarkPit; 