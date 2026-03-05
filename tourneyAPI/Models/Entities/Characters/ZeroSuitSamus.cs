

namespace Entities;

using Enums;
using Enums;

// Defines the competitive profile metadata for this playable character.
public class ZeroSuitSamus : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public ZeroSuitSamus()
    {
        Id = CharacterId.ZeroSuitSamus;
        characterName = CharacterName.ZERO_SUIT_SAMUS;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}