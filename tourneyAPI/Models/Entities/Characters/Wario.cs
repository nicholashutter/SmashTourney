namespace Entities;

using Enums;

public class Wario : Character
{
    public Wario()
    {
        characterName = CharacterName.WARIO;
        archetype = Archetype.HIT_AND_RUN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}