

namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Jigglypuff : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Jigglypuff()
    {
        Id = CharacterId.Jigglypuff;
        characterName = CharacterName.JIGGLYPUFF;
        archetype = Archetype.HIT_AND_RUN;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.BALLOONWEIGHT;
        tierPlacement = TierPlacement.C;
    }

}