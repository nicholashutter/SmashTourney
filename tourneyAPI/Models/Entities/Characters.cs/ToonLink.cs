

namespace Entities;

using Enums;

public class ToonLink : Character
{
    public ToonLink()
    {
        characterName = CharacterName.TOON_LINK;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}