
namespace Entities;

using Enums;


public class Lucas : Character
{
    public Lucas()
    {
        Id = CharacterId.Lucas;
        characterName = CharacterName.LUCAS;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}