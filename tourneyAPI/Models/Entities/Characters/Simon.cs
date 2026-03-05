namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Simon : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Simon()
    {
        Id = CharacterId.Simon;
        characterName = CharacterName.SIMON;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}