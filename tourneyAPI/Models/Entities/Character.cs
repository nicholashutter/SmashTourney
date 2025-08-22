namespace Entities;

using Microsoft.EntityFrameworkCore;
using Enums;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class Character
{
    public CharacterName characterName { get; set; }

    public Archetype archetype { get; set; }

    public FallSpeed fallSpeed { get; set; }

    public TierPlacement tierPlacement { get; set; }

    public WeightClass weightClass { get; set; }


}