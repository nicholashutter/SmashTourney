namespace Entities;

using Enums;

public class MrGameAndWatch : Character
{
    public MrGameAndWatch()
    {
        Id = CharacterId.MrGameAndWatch;
        characterName = CharacterName.MR_GAME_AND_WATCH;
        archetype = Archetype.MIX_UP;
        fallSpeed = FallSpeed.FLOATY;
        weightClass = WeightClass.FEATHERWEIGHT;
        tierPlacement = TierPlacement.S;
    }

}