namespace Entities;

using Enums;

public class Villager : Character
{
    public Villager()
    {
        Id = CharacterId.Villager;
        characterName = CharacterName.VILLAGER;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}