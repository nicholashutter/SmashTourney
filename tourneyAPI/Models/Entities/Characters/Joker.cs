namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Joker : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Joker()
    {
        Id = CharacterId.Joker;
        characterName = CharacterName.JOKER;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.LIGHTWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}