import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Fox: Character =
{
    id: CharacterId.Fox,
    characterName: CharacterName.FOX,
    archetype: Archetype.RUSHDOWN,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.FEATHERWEIGHT,
    tierPlacement: TierPlacement.S
}

export default Fox; 