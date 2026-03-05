namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class MegaMan : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public MegaMan()
    {
        Id = CharacterId.MegaMan;
        characterName = CharacterName.MEGA_MAN;
        archetype = Archetype.ZONER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}