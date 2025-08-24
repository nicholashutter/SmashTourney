namespace Entities;

using Enums;

public class Kirby : Character
{
    public Kirby()
    {
        characterName = CharacterName.KIRBY;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}