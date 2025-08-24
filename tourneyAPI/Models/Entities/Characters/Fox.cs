namespace Entities;

using Enums;

public class Fox : Character
{
    public Fox()
    {
        characterName = CharacterName.FOX;
        archetype = Archetype.RUSHDOWN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}