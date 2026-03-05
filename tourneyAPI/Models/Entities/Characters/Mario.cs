namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Mario : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Mario()
    {
        Id = CharacterId.Mario;
        characterName = CharacterName.MARIO;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}