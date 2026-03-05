namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Zelda : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Zelda()
    {
        Id = CharacterId.Zelda;
        characterName = CharacterName.ZELDA;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}