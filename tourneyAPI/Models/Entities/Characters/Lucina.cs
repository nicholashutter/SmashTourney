namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Lucina : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Lucina()
    {
        Id = CharacterId.Lucina;
        characterName = CharacterName.LUCINA;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}