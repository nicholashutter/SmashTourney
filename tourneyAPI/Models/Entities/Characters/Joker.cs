namespace Entities;

using Enums;


public class Joker : Character
{
    public Joker()
    {
        Id = CharacterId.Joker;
        characterName = CharacterName.JOKER;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}