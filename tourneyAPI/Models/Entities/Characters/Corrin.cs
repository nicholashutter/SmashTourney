namespace Entities;

using Enums;

public class Corrin : Character
{
    public Corrin()
    {
        Id = CharacterId.Corrin;
        characterName = CharacterName.CORRIN;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}