namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class Fox : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Fox()
    {
        Id = CharacterId.Fox;
        characterName = CharacterName.FOX;
        archetype = Archetype.RUSHDOWN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}