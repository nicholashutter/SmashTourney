namespace Entities;

using Enums;

public class KingDedede : Character
{
    public KingDedede()
    {
        characterName = CharacterName.KING_DEDEDE;
        archetype = Archetype.TANK;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}