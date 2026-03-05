namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Luigi : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Luigi()
    {
        Id = CharacterId.Luigi;
        characterName = CharacterName.LUIGI;
        archetype = Archetype.HALF_GRAPPLER;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}