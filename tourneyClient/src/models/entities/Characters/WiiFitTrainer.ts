import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const WiiFitTrainer: Character =
{
    id: CharacterId.WiiFitTrainer,
    characterName: CharacterName.WII_FIT_TRAINER,
    archetype: Archetype.BAIT_AND_PUNISH,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.LIGHTWEIGHT,
    tierPlacement: TierPlacement.C
}

export default WiiFitTrainer; 