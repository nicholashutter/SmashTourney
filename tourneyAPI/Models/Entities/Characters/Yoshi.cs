namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Yoshi : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Yoshi()
    {
        Id = CharacterId.Yoshi;
        characterName = CharacterName.YOSHI;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}