

namespace Entities;

using Enums;

public class Greninja : Character
{
    public Greninja()
    {
        characterName = CharacterName.GRENINJA;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}