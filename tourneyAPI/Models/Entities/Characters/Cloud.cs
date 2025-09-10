namespace Entities;

using Enums;

public class Cloud : Character
{
    public Cloud()
    {
        Id = CharacterId.Cloud;
        characterName = CharacterName.CLOUD;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}