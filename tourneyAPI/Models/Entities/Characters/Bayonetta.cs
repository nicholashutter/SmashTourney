namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Bayonetta : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Bayonetta()
    {
        Id = CharacterId.Bayonetta;
        characterName = CharacterName.BAYONETTA;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}