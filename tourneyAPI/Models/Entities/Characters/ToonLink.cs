

namespace Entities;

using Enums;


public class ToonLink : Character
{
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