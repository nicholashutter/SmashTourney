namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Chrom : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
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