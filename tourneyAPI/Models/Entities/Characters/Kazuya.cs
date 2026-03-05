namespace Entities;

using Enums;


// Defines the competitive profile metadata for this playable character.
public class Kazuya : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public Kazuya()
    {
        Id = CharacterId.Kazuya;
        characterName = CharacterName.KAZUYA;
        archetype = Archetype.GRAPPLER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}