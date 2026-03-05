

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Ganondorf : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Ganondorf()
    {
        Id = CharacterId.Ganondorf;
        characterName = CharacterName.GANONDORF;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.E;
    }

}