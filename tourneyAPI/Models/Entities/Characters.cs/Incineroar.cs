

namespace Entities;

using Enums;

public class Incineroar : Character
{
    public Incineroar()
    {
        characterName = CharacterName.INCINEROAR;
        archetype = Archetype.TANK;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}