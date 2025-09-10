

namespace Entities;

using Enums;

public class DarkSamus : Character
{
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