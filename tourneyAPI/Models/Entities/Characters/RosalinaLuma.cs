namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class RosalinaLuma : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public RosalinaLuma()
    {
        Id = CharacterId.RosalinaAndLuma;
        characterName = CharacterName.ROSALINA_AND_LUMA;
        archetype = Archetype.TAG_TEAM;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}