namespace Entities;

using Enums;


public class KingKRool : Character
{
    public KingKRool()
    {
        Id = CharacterId.KingKRool;
        characterName = CharacterName.KING_K_ROOL;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}