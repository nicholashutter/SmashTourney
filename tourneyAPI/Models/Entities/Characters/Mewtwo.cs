

namespace Entities;

using Enums;

public class Mewtwo : Character
{
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