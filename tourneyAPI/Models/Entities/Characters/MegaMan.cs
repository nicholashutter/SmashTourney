namespace Entities;

using Enums;

public class MegaMan : Character
{
    public MegaMan()
    {
        characterName = CharacterName.MEGA_MAN;
        archetype = Archetype.ZONER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}