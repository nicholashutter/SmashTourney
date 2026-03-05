namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Falco : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Falco()
    {
        Id = CharacterId.Falco;
        characterName = CharacterName.FALCO;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}