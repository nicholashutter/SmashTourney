namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class WiiFitTrainer : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public WiiFitTrainer()
    {
        Id = CharacterId.WiiFitTrainer;
        characterName = CharacterName.WII_FIT_TRAINER;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}