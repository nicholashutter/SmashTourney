namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class KingKRool : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public KingKRool()
    {
        Id = CharacterId.KingKRool;
        characterName = CharacterName.KING_K_ROOL;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}