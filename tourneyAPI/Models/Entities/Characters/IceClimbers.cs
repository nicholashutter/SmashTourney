namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class IceClimbers : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public IceClimbers()
    {
        Id = CharacterId.IceClimbers;
        characterName = CharacterName.ICE_CLIMBERS;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}