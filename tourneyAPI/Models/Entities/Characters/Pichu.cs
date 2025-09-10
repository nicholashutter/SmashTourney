

namespace Entities;

using Enums;

public class Pichu : Character
{
    public Pichu()
    {
        Id = CharacterId.Pichu;
        characterName = CharacterName.PICHU;
        archetype = Archetype.GLASS_CANNON;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}