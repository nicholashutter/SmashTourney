namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class BowserJr : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public BowserJr()
    {
        Id = CharacterId.BowserJr;
        characterName = CharacterName.BOWSER_JR;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}