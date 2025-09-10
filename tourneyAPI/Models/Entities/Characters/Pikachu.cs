

namespace Entities;

using Enums;

public class Pikachu : Character
{
    public Pikachu()
    {
        Id = CharacterId.Pikachu;
        characterName = CharacterName.PIKACHU;
        archetype = Archetype.RUSHDOWN;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}