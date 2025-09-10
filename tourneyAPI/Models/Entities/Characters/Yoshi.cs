namespace Entities;

using Enums;

public class Yoshi : Character
{
    public Yoshi()
    {
        Id = CharacterId.Yoshi;
        characterName = CharacterName.YOSHI;
        archetype = Archetype.ALL_ROUNDER;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}