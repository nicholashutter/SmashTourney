namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Sephiroth : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Sephiroth()
    {
        Id = CharacterId.Sephiroth;
        characterName = CharacterName.SEPHIROTH;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}