namespace Entities;

using Enums;

public class DonkeyKong : Character
{
    public DonkeyKong()
    {
        Id = CharacterId.DonkeyKong;
        characterName = CharacterName.DONKEY_KONG;
        archetype = Archetype.HALF_GRAPPLER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}