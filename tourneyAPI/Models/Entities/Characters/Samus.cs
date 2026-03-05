

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Samus : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Samus()
    {
        Id = CharacterId.Samus;
        characterName = CharacterName.SAMUS;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}