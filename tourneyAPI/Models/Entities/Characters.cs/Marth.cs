namespace Entities;

using Enums;

public class Marth : Character
{
    public Marth()
    {
        characterName = CharacterName.MARTH;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}