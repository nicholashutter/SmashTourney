namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class BanjoKazooie : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public BanjoKazooie()
    {
        Id = CharacterId.BanjoKazooie;
        characterName = CharacterName.BANJO_AND_KAZOOIE;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}