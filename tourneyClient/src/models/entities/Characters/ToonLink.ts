import { CharacterId } from "@/models/Enums/CharacterId";
import { Character } from "../Character"
import { CharacterName } from "@/models/Enums/CharacterName";
import { Archetype } from "@/models/Enums/Archetype";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { WeightClass } from "@/models/Enums/WeightClass";
import { TierPlacement } from "@/models/Enums/TierPlacement";

export const ToonLink: Character =
{
    id: CharacterId.ToonLink,
    characterName: CharacterName.TOON_LINK,
    archetype: Archetype.TURTLE,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.LIGHTWEIGHT,
    tierPlacement: TierPlacement.A
}

export default ToonLink; 