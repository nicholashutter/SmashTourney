

namespace Entities;

using Enums;

public class Jigglypuff : Character
{
    public Jigglypuff()
    {
        characterName = CharacterName.JIGGLYPUFF;
        archetype = Archetype.HIT_AND_RUN;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}