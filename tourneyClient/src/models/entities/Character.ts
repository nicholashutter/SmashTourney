import { Archetype } from "../Enums/Archetype";
import { CharacterId } from "../Enums/CharacterId";
import { CharacterName } from "../Enums/CharacterName";
import { FallSpeed } from "../Enums/FallSpeed";
import { TierPlacement } from "../Enums/TierPlacement";
import { WeightClass } from "../Enums/WeightClass";



export interface Character
{
    id: CharacterId;
    characterName: CharacterName;
    archetype: Archetype;
    fallSpeed: FallSpeed;
    tierPlacement: TierPlacement;
    weightClass: WeightClass;
}