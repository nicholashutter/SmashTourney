namespace Entities;

using Enums;

public class Sora : Character
{
    public Sora()
    {
        Id = CharacterId.Sora;
        characterName = CharacterName.SORA;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}