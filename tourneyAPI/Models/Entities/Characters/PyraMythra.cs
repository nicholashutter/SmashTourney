namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class PyraMythra : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public PyraMythra()
    {
        Id = CharacterId.Pyra;
        characterName = CharacterName.PYRA_AND_MYTHRA;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}