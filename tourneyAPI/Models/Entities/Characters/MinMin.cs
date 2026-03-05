namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class MinMin : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public MinMin()
    {
        Id = CharacterId.MinMin;
        characterName = CharacterName.MIN_MIN;
        archetype = Archetype.ZONER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}