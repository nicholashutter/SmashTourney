namespace Services.Brackets;

using Enums;

// Builds fixed bracket trees before play starts.
//
// The whole bracket is materialized up front, including matches whose players
// are not yet known. Two things depend on that:
//
//   1. A client can draw the complete bracket from the first snapshot. If
//      matches only appeared once both players were decided, the UI could never
//      show a round that had not been reached, which is most of what makes a
//      bracket look like a bracket.
//   2. Progression links (NextMatchForWinner / NextMatchForLoser) can be
//      assigned, because the destination match already exists. Reporting a
//      result then walks a fixed edge instead of pairing whoever happens to be
//      waiting, so bracket shape no longer depends on the order results arrive.
internal static class BracketTreeBuilder
{
    // Returns standard tournament seed positions for a power-of-two bracket.
    //
    // Produced by repeated reflection: a bracket of size 2n is the bracket of
    // size n with every seed s followed by its complement (2n + 1 - s). For
    // eight that gives 1,8,4,5,2,7,3,6 — the ordering that keeps the top seeds
    // apart until the final and pairs each against the weakest slot available.
    //
    // GameService appends synthetic BYE players after the real ones, so byes
    // always hold the highest seed numbers. Ordering this way therefore hands
    // the byes to the top seeds automatically, which is the conventional
    // behaviour and needs no special casing here.
    internal static IReadOnlyList<int> BuildSeedOrder(int bracketSize)
    {
        var order = new List<int> { 1 };

        while (order.Count < bracketSize)
        {
            var complementTotal = (order.Count * 2) + 1;
            var expanded = new List<int>(order.Count * 2);

            foreach (var seed in order)
            {
                expanded.Add(seed);
                expanded.Add(complementTotal - seed);
            }

            order = expanded;
        }

        return order;
    }

    // Returns the number of winners-bracket rounds for a power-of-two size.
    internal static int CountWinnersRounds(int bracketSize)
    {
        var rounds = 0;
        var remaining = bracketSize;

        while (remaining > 1)
        {
            remaining /= 2;
            rounds += 1;
        }

        return rounds;
    }

    // Builds every winners-bracket match and links each winner to its next slot.
    //
    // Returns the rounds as a list of lists so callers can address a specific
    // round without re-filtering the flat match collection.
    internal static List<List<BracketMatchRuntime>> BuildWinnersLane(
        BracketRuntimeState state,
        IReadOnlyList<Guid> seededPlayerIds)
    {
        var bracketSize = seededPlayerIds.Count;
        var totalRounds = CountWinnersRounds(bracketSize);
        var rounds = new List<List<BracketMatchRuntime>>();

        for (var roundIndex = 1; roundIndex <= totalRounds; roundIndex++)
        {
            var matchesInRound = bracketSize >> roundIndex;
            var round = new List<BracketMatchRuntime>(matchesInRound);

            for (var matchIndex = 0; matchIndex < matchesInRound; matchIndex++)
            {
                round.Add(new BracketMatchRuntime
                {
                    MatchId = Guid.NewGuid(),
                    Lane = BracketLane.WINNERS,
                    Round = roundIndex,
                    MatchNumber = ++state.WinnersMatchCounter,
                    Status = BracketMatchStatus.PENDING
                });
            }

            rounds.Add(round);
        }

        // Seats the opening round from the seed order. Every other round starts
        // empty and fills as results arrive.
        var seedOrder = BuildSeedOrder(bracketSize);
        var openingRound = rounds[0];

        for (var matchIndex = 0; matchIndex < openingRound.Count; matchIndex++)
        {
            var playerOneSeed = seedOrder[matchIndex * 2];
            var playerTwoSeed = seedOrder[(matchIndex * 2) + 1];

            openingRound[matchIndex].PlayerOneId = seededPlayerIds[playerOneSeed - 1];
            openingRound[matchIndex].PlayerTwoId = seededPlayerIds[playerTwoSeed - 1];
            openingRound[matchIndex].Status = BracketMatchStatus.READY;
        }

        // Links each match to the one its winner advances into. Two adjacent
        // matches feed the two slots of a single match in the next round.
        for (var roundIndex = 0; roundIndex < rounds.Count - 1; roundIndex++)
        {
            var round = rounds[roundIndex];
            var nextRound = rounds[roundIndex + 1];

            for (var matchIndex = 0; matchIndex < round.Count; matchIndex++)
            {
                round[matchIndex].NextMatchForWinner = nextRound[matchIndex / 2].MatchId;
                round[matchIndex].NextSlotForWinner = (matchIndex % 2) + 1;
            }
        }

        foreach (var round in rounds)
        {
            state.Matches.AddRange(round);
        }

        return rounds;
    }

    // Builds every losers-bracket match for a double-elimination tree.
    //
    // The losers bracket alternates two kinds of round. A minor round pairs
    // survivors already in the lane against each other; the major round that
    // follows feeds in the losers of one winners-bracket round. Both hold the
    // same number of matches, and the count halves after each major round.
    //
    // A bracket of size two has no losers rounds at all — its single loser goes
    // straight to the grand final.
    internal static List<List<BracketMatchRuntime>> BuildLosersLane(
        BracketRuntimeState state,
        int bracketSize)
    {
        var winnersRounds = CountWinnersRounds(bracketSize);
        var rounds = new List<List<BracketMatchRuntime>>();

        for (var stage = 1; stage < winnersRounds; stage++)
        {
            var matchesInRound = bracketSize >> (stage + 1);

            // Minor round, then the major round that receives winners-bracket
            // losers. Both are the same width.
            for (var roundKind = 0; roundKind < 2; roundKind++)
            {
                var round = new List<BracketMatchRuntime>(matchesInRound);

                for (var matchIndex = 0; matchIndex < matchesInRound; matchIndex++)
                {
                    round.Add(new BracketMatchRuntime
                    {
                        MatchId = Guid.NewGuid(),
                        Lane = BracketLane.LOSERS,
                        Round = rounds.Count + 1,
                        MatchNumber = ++state.LosersMatchCounter,
                        Status = BracketMatchStatus.PENDING
                    });
                }

                rounds.Add(round);
            }
        }

        // Links losers-lane winners forward. A minor round feeds the major round
        // beside it one-for-one; a major round feeds the next minor round two
        // matches into one.
        for (var roundIndex = 0; roundIndex < rounds.Count - 1; roundIndex++)
        {
            var round = rounds[roundIndex];
            var nextRound = rounds[roundIndex + 1];
            var isMinorRound = roundIndex % 2 == 0;

            for (var matchIndex = 0; matchIndex < round.Count; matchIndex++)
            {
                if (isMinorRound)
                {
                    round[matchIndex].NextMatchForWinner = nextRound[matchIndex].MatchId;
                    round[matchIndex].NextSlotForWinner = 1;
                }
                else
                {
                    round[matchIndex].NextMatchForWinner = nextRound[matchIndex / 2].MatchId;
                    round[matchIndex].NextSlotForWinner = (matchIndex % 2) + 1;
                }
            }
        }

        foreach (var round in rounds)
        {
            state.Matches.AddRange(round);
        }

        return rounds;
    }

    // Routes winners-bracket losers into their reserved losers-bracket slots.
    //
    // Round one drops into the opening minor round, filling both slots. Every
    // later winners round drops into the major round for its stage, taking the
    // second slot behind the survivor already there.
    //
    // The drop order is reversed on each successive round. Without that, the
    // player knocked out of a winners match repeatedly meets the same opponent
    // they would have met anyway, and the losers bracket degenerates into a
    // rematch of the winners bracket.
    internal static void LinkWinnersLosersToLosersLane(
        List<List<BracketMatchRuntime>> winnersRounds,
        List<List<BracketMatchRuntime>> losersRounds)
    {
        if (losersRounds.Count == 0)
        {
            return;
        }

        // Winners round one fills the opening minor round, two losers per match.
        var openingMinorRound = losersRounds[0];
        var openingWinnersRound = winnersRounds[0];

        for (var matchIndex = 0; matchIndex < openingWinnersRound.Count; matchIndex++)
        {
            var target = openingMinorRound[matchIndex / 2];
            openingWinnersRound[matchIndex].NextMatchForLoser = target.MatchId;
            openingWinnersRound[matchIndex].NextSlotForLoser = (matchIndex % 2) + 1;
        }

        // Each later winners round fills the major round for its stage.
        for (var winnersRoundIndex = 1; winnersRoundIndex < winnersRounds.Count; winnersRoundIndex++)
        {
            var majorRoundIndex = (winnersRoundIndex * 2) - 1;
            if (majorRoundIndex >= losersRounds.Count)
            {
                break;
            }

            var winnersRound = winnersRounds[winnersRoundIndex];
            var majorRound = losersRounds[majorRoundIndex];
            var reverseOrder = winnersRoundIndex % 2 == 1;

            for (var matchIndex = 0; matchIndex < winnersRound.Count; matchIndex++)
            {
                var targetIndex = reverseOrder
                    ? majorRound.Count - 1 - (matchIndex % majorRound.Count)
                    : matchIndex % majorRound.Count;

                winnersRound[matchIndex].NextMatchForLoser = majorRound[targetIndex].MatchId;
                winnersRound[matchIndex].NextSlotForLoser = 2;
            }
        }
    }

    // Places a player into a reserved slot and readies the match when both are set.
    internal static void SeatPlayer(BracketMatchRuntime match, int slot, Guid playerId)
    {
        if (slot == 2)
        {
            match.PlayerTwoId = playerId;
        }
        else
        {
            match.PlayerOneId = playerId;
        }

        if (match.PlayerOneId is not null &&
            match.PlayerTwoId is not null &&
            match.Status is BracketMatchStatus.PENDING)
        {
            match.Status = BracketMatchStatus.READY;
        }
    }
}
