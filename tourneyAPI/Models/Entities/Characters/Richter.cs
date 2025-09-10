namespace Entities;

using Enums;

public class Richter : Character
{
    public Richter()
    {
        Id = CharacterId.Richter;
        characterName = CharacterName.RICHTER;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}