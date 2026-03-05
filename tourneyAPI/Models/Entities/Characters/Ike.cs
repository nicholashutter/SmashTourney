namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Ike : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Ike()
    {
        Id = CharacterId.Ike;
        characterName = CharacterName.IKE;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}