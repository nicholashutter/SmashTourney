namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Wario : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Wario()
    {
        Id = CharacterId.Wario;
        characterName = CharacterName.WARIO;
        archetype = Archetype.HIT_AND_RUN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}