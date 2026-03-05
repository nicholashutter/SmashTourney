namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Cloud : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Cloud()
    {
        Id = CharacterId.Cloud;
        characterName = CharacterName.CLOUD;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}