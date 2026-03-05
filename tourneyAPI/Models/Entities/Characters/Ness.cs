
namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Ness : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Ness()
    {
        Id = CharacterId.Ness;
        characterName = CharacterName.NESS;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}