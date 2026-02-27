import { Archetype } from "@/models/Enums/Archetype";
import { CharacterId } from "@/models/Enums/CharacterId";
import { CharacterName } from "@/models/Enums/CharacterName";
import { FallSpeed } from "@/models/Enums/FallSpeed";
import { TierPlacement } from "@/models/Enums/TierPlacement";
import { WeightClass } from "@/models/Enums/WeightClass";



export interface Character
{
    id: CharacterId;
    characterName: CharacterName;
    archetype: Archetype;
    fallSpeed: FallSpeed;
    tierPlacement: TierPlacement;
    weightClass: WeightClass;
}