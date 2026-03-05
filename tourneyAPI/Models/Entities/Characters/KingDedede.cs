namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class KingDedede : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public KingDedede()
    {
        Id = CharacterId.KingDedede;
        characterName = CharacterName.KING_DEDEDE;
        archetype = Archetype.TANK;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}