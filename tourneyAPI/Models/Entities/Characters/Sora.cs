namespace Entities;

using Enums;

public class Sora : Character
{
    public Sora()
    {
        characterName = CharacterName.SORA;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}