namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Robin : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Robin()
    {
        Id = CharacterId.Robin;
        characterName = CharacterName.ROBIN;
        archetype = Archetype.ZONER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}