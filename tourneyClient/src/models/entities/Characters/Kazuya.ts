import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Kazuya: Character =
{
    id: CharacterId.Kazuya,
    characterName: CharacterName.KAZUYA,
    archetype: Archetype.GRAPPLER,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.HEAVYWEIGHT,
    tierPlacement: TierPlacement.S
}

export default Kazuya; 