import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const RosalinaLuma: Character =
{
    id: CharacterId.RosalinaAndLuma,
    characterName: CharacterName.ROSALINA_AND_LUMA,
    archetype: Archetype.TAG_TEAM,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.FEATHERWEIGHT,
    tierPlacement: TierPlacement.B
}

export default RosalinaLuma; 