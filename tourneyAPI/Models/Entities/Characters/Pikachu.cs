

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Pikachu : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Pikachu()
    {
        Id = CharacterId.Pikachu;
        characterName = CharacterName.PIKACHU;
        archetype = Archetype.RUSHDOWN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}