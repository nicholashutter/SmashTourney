namespace Entities;

using Enums;

public class Wolf : Character
{
    public Wolf()
    {
        Id = CharacterId.Wolf;
        characterName = CharacterName.WOLF;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}