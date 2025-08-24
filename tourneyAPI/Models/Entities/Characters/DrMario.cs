namespace Entities;

using Enums;

public class DrMario : Character
{
    public DrMario()
    {
        characterName = CharacterName.DR_MARIO;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}