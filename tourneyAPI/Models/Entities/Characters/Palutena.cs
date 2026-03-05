namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Palutena : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Palutena()
    {
        Id = CharacterId.Palutena;
        characterName = CharacterName.PALUTENA;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}