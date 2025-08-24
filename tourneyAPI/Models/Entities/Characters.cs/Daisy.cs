namespace Entities;

using Enums;

public class Daisy : Character
{
    public Daisy()
    {
        characterName = CharacterName.DAISY;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}