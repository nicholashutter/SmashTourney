namespace Entities;

using Enums;


public class Kazuya : Character
{
    public Kazuya()
    {
        Id = CharacterId.Kazuya;
        characterName = CharacterName.KAZUYA;
        archetype = Archetype.GRAPPLER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}