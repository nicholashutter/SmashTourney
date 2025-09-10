namespace Entities;

using Enums;
using Enums;

public class Terry : Character
{
    public Terry()
    {
        Id = CharacterId.Terry;
        characterName = CharacterName.TERRY;
        archetype = Archetype.FOOTSIES;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}