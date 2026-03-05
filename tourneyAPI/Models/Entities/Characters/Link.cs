namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Link : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Link()
    {
        Id = CharacterId.Link;
        characterName = CharacterName.LINK;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}