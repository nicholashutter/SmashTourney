namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Roy : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Roy()
    {
        Id = CharacterId.Roy;
        characterName = CharacterName.ROY;
        archetype = Archetype.RUSHDOWN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}