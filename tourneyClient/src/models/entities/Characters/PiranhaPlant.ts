import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { Archetype } from "@/models/Enums/Archetype";
import { CharacterName } from "@/models/Enums/CharacterName";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const PiranhaPlant: Character =
{
    id: CharacterId.PiranhaPlant,
    characterName: CharacterName.PIRANHA_PLANT,
    archetype: Archetype.TRAPPER,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.HEAVYWEIGHT,
    tierPlacement: TierPlacement.D
}

export default PiranhaPlant; 