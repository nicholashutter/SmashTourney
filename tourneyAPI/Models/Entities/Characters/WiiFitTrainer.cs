namespace Entities;

using Enums;

public class WiiFitTrainer : Character
{
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