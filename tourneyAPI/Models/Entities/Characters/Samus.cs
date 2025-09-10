

namespace Entities;

using Enums;

public class Samus : Character
{
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