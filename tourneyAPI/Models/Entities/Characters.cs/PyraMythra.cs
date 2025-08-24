namespace Entities;

using Enums;

public class PyraMythra : Character
{
    public PyraMythra()
    {
        characterName = CharacterName.PYRA_AND_MYTHRA;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}