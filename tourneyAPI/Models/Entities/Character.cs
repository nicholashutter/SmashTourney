namespace Entities;

using Enums;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
// Represents the chosen fighter attributes for a player.
public class Character
{
    public Guid Id { get; set; }
    public CharacterName characterName { get; set; }

    public Archetype archetype { get; set; }

    public FallSpeed fallSpeed { get; set; }

    public TierPlacement tierPlacement { get; set; }

    public WeightClass weightClass { get; set; }

}