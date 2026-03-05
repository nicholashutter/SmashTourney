

namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Lucario : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Lucario()
    {
        Id = CharacterId.Lucario;
        characterName = CharacterName.LUCARIO;
        archetype = Archetype.AURA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}