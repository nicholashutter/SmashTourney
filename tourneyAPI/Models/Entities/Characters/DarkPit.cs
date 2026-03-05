namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class DarkPit : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public DarkPit()
    {
        Id = CharacterId.DarkPit;
        characterName = CharacterName.DARK_PIT;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}