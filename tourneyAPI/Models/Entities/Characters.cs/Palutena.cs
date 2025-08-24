namespace Entities;

using Enums;

public class Palutena : Character
{
    public Palutena()
    {
        characterName = CharacterName.PALUTENA;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}