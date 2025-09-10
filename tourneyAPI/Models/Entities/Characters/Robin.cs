namespace Entities;

using Enums;

public class Robin : Character
{
    public Robin()
    {
        Id = CharacterId.Robin;
        characterName = CharacterName.ROBIN;
        archetype = Archetype.ZONER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}