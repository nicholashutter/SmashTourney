import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Sora: Character =
{
    id: CharacterId.Sora,
    characterName: CharacterName.SORA,
    archetype: Archetype.MIX_UP,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.LIGHTWEIGHT,
    tierPlacement: TierPlacement.A
}

export default Sora; 