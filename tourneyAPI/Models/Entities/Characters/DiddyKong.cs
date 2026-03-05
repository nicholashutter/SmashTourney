namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class DiddyKong : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public DiddyKong()
    {
        Id = CharacterId.DiddyKong;
        characterName = CharacterName.DIDDY_KONG;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}