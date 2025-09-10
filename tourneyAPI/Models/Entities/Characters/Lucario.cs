

namespace Entities;

using Enums;


public class Lucario : Character
{
    public Lucario()
    {
        Id = CharacterId.Lucario;
        characterName = CharacterName.LUCARIO;
        archetype = Archetype.AURA;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}