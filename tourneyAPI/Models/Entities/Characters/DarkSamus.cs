

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class DarkSamus : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public DarkSamus()
    {
        Id = CharacterId.DarkSamus;
        characterName = CharacterName.DARK_SAMUS;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}