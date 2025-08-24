namespace Entities;

using Enums;

public class DuckHunt : Character
{
    public DuckHunt()
    {
        characterName = CharacterName.DUCK_HUNT;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}