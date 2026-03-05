namespace Entities;

using Enums;

// Defines the competitive profile metadata for this playable character.
public class MrGameAndWatch : Character
{
    // Initializes this character's default competitive attributes for matchmaking and tier logic.
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