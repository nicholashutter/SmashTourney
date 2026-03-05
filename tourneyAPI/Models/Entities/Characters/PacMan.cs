namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class PacMan : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public PacMan()
    {
        Id = CharacterId.PacMan;
        characterName = CharacterName.PAC_MAN;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}