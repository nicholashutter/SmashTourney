namespace Entities;

using Enums;

public class DarkPit : Character
{
    public DarkPit()
    {
        characterName = CharacterName.DARK_PIT;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}