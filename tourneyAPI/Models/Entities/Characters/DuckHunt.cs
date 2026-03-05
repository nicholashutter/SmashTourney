namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class DuckHunt : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public DuckHunt()
    {
        Id = CharacterId.DuckHunt;
        characterName = CharacterName.DUCK_HUNT;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}