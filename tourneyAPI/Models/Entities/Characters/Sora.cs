namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Sora : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Sora()
    {
        Id = CharacterId.Sora;
        characterName = CharacterName.SORA;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}