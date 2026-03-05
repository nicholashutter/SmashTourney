namespace Entities;

using Enums;
using Enums;

// Defines the competitive profile metadata for this playable character.
public class Terry : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Terry()
    {
        Id = CharacterId.Terry;
        characterName = CharacterName.TERRY;
        archetype = Archetype.FOOTSIES;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}