namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Peach : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Peach()
    {
        Id = CharacterId.Peach;
        characterName = CharacterName.PEACH;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}