import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const Bayonetta: Character =
{
    id: CharacterId.Bayonetta,
    characterName: CharacterName.BAYONETTA,
    archetype: Archetype.MIX_UP,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.FEATHERWEIGHT,
    tierPlacement: TierPlacement.A
}

export default Bayonetta; 