namespace Entities;

using Enums;

public class Roy : Character
{
    public Roy()
    {
        characterName = CharacterName.ROY;
        archetype = Archetype.RUSHDOWN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}