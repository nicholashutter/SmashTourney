namespace Entities;

using Enums;

public class Peach : Character
{
    public Peach()
    {
        characterName = CharacterName.PEACH;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}