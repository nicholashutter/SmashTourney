import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Jigglypuff: Character =
{
    id: CharacterId.Jigglypuff,
    characterName: CharacterName.JIGGLYPUFF,
    archetype: Archetype.HIT_AND_RUN,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.BALLOONWEIGHT,
    tierPlacement: TierPlacement.C
}

export default Jigglypuff; 