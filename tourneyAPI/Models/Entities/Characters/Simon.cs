namespace Entities;

using Enums;

public class Simon : Character
{
    public Simon()
    {
        Id = CharacterId.Simon;
        characterName = CharacterName.SIMON;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}