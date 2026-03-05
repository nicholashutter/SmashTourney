namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Bowser : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Bowser()
    {
        Id = CharacterId.Bowser;
        characterName = CharacterName.BOWSER;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.SUPER_HEAVYWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}