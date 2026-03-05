namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Villager : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Villager()
    {
        Id = CharacterId.Villager;
        characterName = CharacterName.VILLAGER;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}