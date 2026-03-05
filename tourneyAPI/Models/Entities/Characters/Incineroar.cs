

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Incineroar : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Incineroar()
    {
        Id = CharacterId.Incineroar;
        characterName = CharacterName.INCINEROAR;
        archetype = Archetype.TANK;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}