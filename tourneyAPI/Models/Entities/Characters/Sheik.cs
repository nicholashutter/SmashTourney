namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Sheik : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Sheik()
    {
        Id = CharacterId.Sheik;
        characterName = CharacterName.SHEIK;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}