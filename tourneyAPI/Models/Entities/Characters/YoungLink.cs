

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class YoungLink : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public YoungLink()
    {
        Id = CharacterId.YoungLink;
        characterName = CharacterName.YOUNG_LINK;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}