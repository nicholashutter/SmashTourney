using CustomExceptions;
using Entities;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace Validators;

[ExcludeFromCodeCoverage]
public class PlayersValidator()
{
    public static void Validate(List<Player> players, string TAG)
    {
        foreach (var player in players)
        {
            PlayerValidator.Validate(player, TAG);
        }
    }
}