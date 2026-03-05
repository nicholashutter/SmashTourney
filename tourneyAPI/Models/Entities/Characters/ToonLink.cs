

namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class ToonLink : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public ToonLink()
    {
        Id = CharacterId.ToonLink;
        characterName = CharacterName.TOON_LINK;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}