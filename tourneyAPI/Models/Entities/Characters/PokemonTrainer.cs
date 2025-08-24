

namespace Entities;

using Enums;

public class PokemonTrainer : Character
{
    public PokemonTrainer()
    {
        characterName = CharacterName.POKEMON_TRAINER;
        archetype = Archetype.DYNAMIC;
        fallSpeed = FallSpeed.FAST_FALLERS;
        weightClass = WeightClass.MIDDLEWEIGHT;
        tierPlacement = TierPlacement.A;
    }

}