namespace Entities;

using Enums;

public class Sheik : Character
{
    public Sheik()
    {
        characterName = CharacterName.SHEIK;
        archetype = Archetype.NINJA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}