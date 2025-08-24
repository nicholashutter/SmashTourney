namespace Entities;

using Enums;

public class RosalinaLuma : Character
{
    public RosalinaLuma()
    {
        characterName = CharacterName.ROSALINA_AND_LUMA;
        archetype = Archetype.TAG_TEAM;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}