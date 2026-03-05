

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Ridley : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Ridley()
    {
        Id = CharacterId.Ridley;
        characterName = CharacterName.RIDLEY;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}