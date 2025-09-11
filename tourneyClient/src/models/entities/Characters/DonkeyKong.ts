import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";
import { CharacterName } from "@/models/Enums/CharacterName";

export const DonkeyKong: Character =
{
    id: CharacterId.DonkeyKong,
    characterName: CharacterName.DONKEY_KONG,
    archetype: Archetype.HALF_GRAPPLER,
    fallSpeed: FallSpeed.FAST_FALLERS,
    weightClass: WeightClass.FEATHERWEIGHT,
    tierPlacement: TierPlacement.A
}

export default DonkeyKong; 