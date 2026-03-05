namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Corrin : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Corrin()
    {
        Id = CharacterId.Corrin;
        characterName = CharacterName.CORRIN;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}