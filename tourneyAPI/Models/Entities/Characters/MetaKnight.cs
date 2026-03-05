namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class MetaKnight : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public MetaKnight()
    {
        Id = CharacterId.MetaKnight;
        characterName = CharacterName.META_KNIGHT;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}