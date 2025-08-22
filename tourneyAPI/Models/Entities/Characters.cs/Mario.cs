namespace Entities;

using Enums;

public class Mario : Character
{
    public Mario()
    {
        characterName = CharacterName.MARIO;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}