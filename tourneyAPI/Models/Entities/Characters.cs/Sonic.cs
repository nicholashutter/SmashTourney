namespace Entities;

using Enums;

public class Sonic : Character
{
    public Sonic()
    {
        characterName = CharacterName.SONIC;
        archetype = Archetype.HIT_AND_RUN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}