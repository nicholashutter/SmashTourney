namespace Entities;

using Enums;

public class Snake : Character
{
    public Snake()
    {
        characterName = CharacterName.SNAKE;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}