namespace Entities;

using Enums;

public class Pit : Character
{
    public Pit()
    {
        characterName = CharacterName.PIT;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}