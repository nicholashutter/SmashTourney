

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Greninja : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Greninja()
    {
        Id = CharacterId.Greninja;
        characterName = CharacterName.GRENINJA;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}