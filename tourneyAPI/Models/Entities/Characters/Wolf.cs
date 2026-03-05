namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Wolf : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
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