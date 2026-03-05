namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Snake : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Snake()
    {
        Id = CharacterId.Snake;
        characterName = CharacterName.SNAKE;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}