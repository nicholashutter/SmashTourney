namespace Entities;

using Enums;

public class Bowser : Character
{
    public Bowser()
    {
        Id = CharacterId.Bowser;
        characterName = CharacterName.BOWSER;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.SUPER_HEAVYWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}