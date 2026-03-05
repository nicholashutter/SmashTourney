namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Daisy : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Daisy()
    {
        Id = CharacterId.Daisy;
        characterName = CharacterName.DAISY;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}