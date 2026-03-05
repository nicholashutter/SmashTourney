
namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Lucas : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Lucas()
    {
        Id = CharacterId.Lucas;
        characterName = CharacterName.LUCAS;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.B;
    }

}