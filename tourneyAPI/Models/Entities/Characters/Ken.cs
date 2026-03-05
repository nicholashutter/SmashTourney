namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Ken : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Ken()
    {
        Id = CharacterId.Ken;
        characterName = CharacterName.KEN;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}