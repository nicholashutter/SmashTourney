namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class LittleMac : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public LittleMac()
    {
        Id = CharacterId.Link;
        characterName = CharacterName.LITTLE_MAC;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.E;
    }

}