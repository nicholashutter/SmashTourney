namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class DrMario : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public DrMario()
    {
        Id = CharacterId.DrMario;
        characterName = CharacterName.DR_MARIO;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}