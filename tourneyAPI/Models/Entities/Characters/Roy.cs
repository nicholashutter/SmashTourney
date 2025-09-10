namespace Entities;

using Enums;

public class Roy : Character
{
    public Roy()
    {
        Id = CharacterId.Roy;
        characterName = CharacterName.ROY;
        archetype = Archetype.RUSHDOWN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}