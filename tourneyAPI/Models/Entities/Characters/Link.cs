namespace Entities;

using Enums;


public class Link : Character
{
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