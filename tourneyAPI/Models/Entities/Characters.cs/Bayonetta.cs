namespace Entities;

using Enums;

public class Bayonetta : Character
{
    public Bayonetta()
    {
        characterName = CharacterName.BAYONETTA;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}