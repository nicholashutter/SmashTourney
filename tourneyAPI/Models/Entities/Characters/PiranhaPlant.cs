namespace Entities;

using Enums;

public class PiranhaPlant : Character
{
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