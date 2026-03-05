namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Hero : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Hero()
    {
        Id = CharacterId.Hero;
        characterName = CharacterName.HERO;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}