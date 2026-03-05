

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Mewtwo : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Mewtwo()
    {
        Id = CharacterId.Mewtwo;
        characterName = CharacterName.MEWTWO;
        archetype = Archetype.GLASS_CANNON;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}