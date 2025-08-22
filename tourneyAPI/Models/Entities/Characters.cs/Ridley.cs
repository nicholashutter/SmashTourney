

namespace Entities;

using Enums;

public class Ridley : Character
{
    public Ridley()
    {
        characterName = CharacterName.RIDLEY;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}