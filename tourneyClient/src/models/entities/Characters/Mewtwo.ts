import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Mewtwo: Character =
{
    id: CharacterId.Mewtwo,
    characterName: CharacterName.MEWTWO,
    archetype: Archetype.GLASS_CANNON,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.FEATHERWEIGHT,
    tierPlacement: TierPlacement.C
}

export default Mewtwo; 