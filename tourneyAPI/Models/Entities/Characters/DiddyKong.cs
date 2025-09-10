namespace Entities;

using Enums;

public class DiddyKong : Character
{
    public DiddyKong()
    {
        Id = CharacterId.DiddyKong;
        characterName = CharacterName.DIDDY_KONG;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}