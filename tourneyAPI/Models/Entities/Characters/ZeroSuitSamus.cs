

namespace Entities;

using Enums;

public class ZeroSuitSamus : Character
{
    public ZeroSuitSamus()
    {
        characterName = CharacterName.ZERO_SUIT_SAMUS;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}