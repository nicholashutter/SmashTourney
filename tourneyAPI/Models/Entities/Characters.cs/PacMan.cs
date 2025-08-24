namespace Entities;

using Enums;

public class PacMan : Character
{
    public PacMan()
    {
        characterName = CharacterName.PAC_MAN;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}