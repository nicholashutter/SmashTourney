namespace Entities;

using Enums;

public class Hero : Character
{
    public Hero()
    {
        characterName = CharacterName.HERO;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}