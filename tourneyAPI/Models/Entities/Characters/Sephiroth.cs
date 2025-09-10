namespace Entities;

using Enums;

public class Sephiroth : Character
{
    public Sephiroth()
    {
        Id = CharacterId.Sephiroth;
        characterName = CharacterName.SEPHIROTH;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}