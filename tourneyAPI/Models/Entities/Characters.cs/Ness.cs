
namespace Entities;

using Enums;

public class Ness : Character
{
    public Ness()
    {
        characterName = CharacterName.NESS;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}