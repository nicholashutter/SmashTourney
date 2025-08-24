namespace Entities;

using Enums;

public class Lucina : Character
{
    public Lucina()
    {
        characterName = CharacterName.LUCINA;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}