namespace Entities;

using Enums;

public class MetaKnight : Character
{
    public MetaKnight()
    {
        characterName = CharacterName.META_KNIGHT;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}