namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class DonkeyKong : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public DonkeyKong()
    {
        Id = CharacterId.DonkeyKong;
        characterName = CharacterName.DONKEY_KONG;
        archetype = Archetype.HALF_GRAPPLER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}