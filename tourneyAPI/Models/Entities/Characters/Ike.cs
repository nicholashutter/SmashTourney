namespace Entities;

using Enums;

public class Ike : Character
{
    public Ike()
    {
        characterName = CharacterName.IKE;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}