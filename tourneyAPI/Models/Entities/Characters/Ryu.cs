namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Ryu : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Ryu()
    {
        Id = CharacterId.Ryu;
        characterName = CharacterName.RYU;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}
