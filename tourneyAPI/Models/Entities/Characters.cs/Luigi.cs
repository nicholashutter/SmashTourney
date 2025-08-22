namespace Entities;

using Enums;

public class Luigi : Character
{
    public Luigi()
    {
        characterName = CharacterName.LUIGI;
        archetype = Archetype.HALF_GRAPPLER;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}