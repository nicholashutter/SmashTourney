namespace Entities;

using Enums;

public class Ryu : Character
{
    public Ryu()
    {
        characterName = CharacterName.RYU;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}
