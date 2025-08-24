namespace Entities;

using Enums;

public class LittleMac : Character
{
    public LittleMac()
    {
        characterName = CharacterName.LITTLE_MAC;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.E;
    }

}