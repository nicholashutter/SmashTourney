namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Marth : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Marth()
    {
        Id = CharacterId.Marth;
        characterName = CharacterName.MARTH;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}