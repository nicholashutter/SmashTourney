

namespace Entities;

using Enums;

public class YoungLink : Character
{
    public YoungLink()
    {
        characterName = CharacterName.YOUNG_LINK;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}