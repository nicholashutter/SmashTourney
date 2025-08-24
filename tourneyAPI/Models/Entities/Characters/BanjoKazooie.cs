namespace Entities;

using Enums;

public class BanjoKazooie : Character
{
    public BanjoKazooie()
    {
        characterName = CharacterName.BANJO_AND_KAZOOIE;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}