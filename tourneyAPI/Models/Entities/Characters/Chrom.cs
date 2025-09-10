namespace Entities;

using Enums;

public class Chrom : Character
{
    public Chrom()
    {
        Id = CharacterId.Chrom;
        characterName = CharacterName.CHROM;
        archetype = Archetype.GLASS_CANNON;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}