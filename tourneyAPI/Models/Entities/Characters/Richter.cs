namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Richter : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Richter()
    {
        Id = CharacterId.Richter;
        characterName = CharacterName.RICHTER;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}