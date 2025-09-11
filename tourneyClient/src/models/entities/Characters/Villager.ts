import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Villager: Character =
{
    id: CharacterId.Villager,
    characterName: CharacterName.VILLAGER,
    archetype: Archetype.TRAPPER,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.BALLOONWEIGHT,
    tierPlacement: TierPlacement.D
}

export default Villager; 