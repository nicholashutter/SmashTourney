

namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class PokemonTrainer : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
    public PokemonTrainer()
    {
        Id = CharacterId.PokémonTrainer;
        characterName = CharacterName.POKEMON_TRAINER;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}