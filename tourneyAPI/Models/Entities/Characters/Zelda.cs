namespace Entities;

using Enums;

public class Zelda : Character
{
    public Zelda()
    {
        Id = CharacterId.Zelda;
        characterName = CharacterName.ZELDA;
        archetype = Archetype.TURTLE;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}