namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Kirby : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Kirby()
    {
        Id = CharacterId.Kirby;
        characterName = CharacterName.KIRBY;
        archetype = Archetype.BAIT_AND_PUNISH;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}