namespace Entities;

using Enums;

public class Terry : Character
{
    public Terry()
    {
        characterName = CharacterName.TERRY;
        archetype = Archetype.FOOTSIES;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}