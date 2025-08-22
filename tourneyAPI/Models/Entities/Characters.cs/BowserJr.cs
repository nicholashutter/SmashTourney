namespace Entities;

using Enums;

public class BowserJr : Character
{
    public BowserJr()
    {
        characterName = CharacterName.BOWSER_JR;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}