namespace Entities;

using Enums;

public class Falco : Character
{
    public Falco()
    {
        Id = CharacterId.Falco;
        characterName = CharacterName.FALCO;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}