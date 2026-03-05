

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Pichu : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Pichu()
    {
        Id = CharacterId.Pichu;
        characterName = CharacterName.PICHU;
        archetype = Archetype.GLASS_CANNON;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}