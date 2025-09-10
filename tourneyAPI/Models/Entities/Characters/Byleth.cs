namespace Entities;

using Enums;

public class Byleth : Character
{
    public Byleth ()
    {
        Id = CharacterId.Byleth;
        characterName = CharacterName.BYLETH;
        archetype = Archetype.PRECISION;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}