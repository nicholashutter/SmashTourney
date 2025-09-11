import { CharacterName } from '@/models/Enums/CharacterName';
import { Character } from '../Character';
import { CharacterId } from '@/models/Enums/CharacterId';
import { Archetype } from '@/models/Enums/Archetype';
import { FallSpeed } from '@/models/Enums/FallSpeed';
import { WeightClass } from '@/models/Enums/WeightClass';
import { TierPlacement } from '@/models/Enums/TierPlacement';

export const DarkSamus: Character =
{
    id: CharacterId.DarkSamus,
    characterName: CharacterName.DARK_SAMUS,
    archetype: Archetype.TURTLE,
    fallSpeed: FallSpeed.FLOATY,
    weightClass: WeightClass.MIDDLEWEIGHT,
    tierPlacement: TierPlacement.A
}

export default DarkSamus; 