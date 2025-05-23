namespace Entities;

using Microsoft.EntityFrameworkCore;
using Enums;

public class Character
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = default!;

    public Archetype archetype { get; set; }

    public FallSpeed fallSpeed { get; set; }

    public TierPlacement tierPlacement { get; set; }

    public WeightClass weightClass { get; set; }


}