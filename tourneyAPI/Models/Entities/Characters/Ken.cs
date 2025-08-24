namespace Entities;

using Enums;

public class Ken : Character
{
    public Ken()
    {
        characterName = CharacterName.KEN;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}