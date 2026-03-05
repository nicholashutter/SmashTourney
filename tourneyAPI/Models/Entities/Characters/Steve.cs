namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Steve : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Steve()
    {
        Id = CharacterId.Steve;
        characterName = CharacterName.STEVE;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}