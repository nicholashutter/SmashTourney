namespace Entities;

using Enums;

public class IceClimbers : Character
{
    public IceClimbers()
    {
        characterName = CharacterName.ICE_CLIMBERS;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}