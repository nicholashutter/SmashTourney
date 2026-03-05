namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Sonic : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Sonic()
    {
        Id = CharacterId.Sonic;
        characterName = CharacterName.SONIC;
        archetype = Archetype.HIT_AND_RUN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}