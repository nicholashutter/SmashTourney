namespace Entities;

using Enums;

public class Steve : Character
{
    public Steve()
    {
        characterName = CharacterName.STEVE;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}