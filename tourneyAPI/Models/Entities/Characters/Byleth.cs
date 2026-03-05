namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Byleth : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Byleth ()
    {
        Id = CharacterId.Byleth;
        characterName = CharacterName.BYLETH;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}