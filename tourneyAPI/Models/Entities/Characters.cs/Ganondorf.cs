

namespace Entities;

using Enums;

public class Ganondorf : Character
{
    public Ganondorf()
    {
        characterName = CharacterName.GANONDORF;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.E;
    }

}