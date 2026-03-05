namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class PiranhaPlant : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public PiranhaPlant()
    {
        Id = CharacterId.PiranhaPlant;
        characterName = CharacterName.PIRANHA_PLANT;
        archetype = Archetype.TRAPPER;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.HEAVYWEIGHT;
        tierPlacement = TierPlacement.D;
    }

}