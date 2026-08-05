import { expect, test } from "vitest";
import { getVoteFeedbackFromError, getVoteFeedbackFromResponse } from "../src/services/matchVoteFeedback";

const buildResponseFeedback = (status: "PENDING" | "COMMITTED") =>
{
    return getVoteFeedbackFromResponse({
        gameId: "game-id",
        matchId: "match-id",
        status,
        voteCount: status === "PENDING" ? 1 : 2,
        committedWinnerPlayerId: status === "COMMITTED" ? "winner-id" : undefined
    });
};

const buildErrorFeedback = (status: string, code = 409) =>
{
    return getVoteFeedbackFromError(`HTTP ${code}: {"status":"${status}"}`);
};

// Verifies the PENDING response yields a waiting notice and locks voting.
test("pending response returns waiting notice and locks voting", () =>
{
    expect(buildResponseFeedback("PENDING")).toEqual({
        noticeMessage: "Your vote is locked in. Waiting for the other player to vote.",
        refreshMatchData: false,
        clearSelectedWinner: true,
        lockVoteForCurrentMatch: true
    });
});

// Verifies the COMMITTED response confirms the result and refreshes match data.
test("committed response returns confirmation alert and refreshes match data", () =>
{
    expect(buildResponseFeedback("COMMITTED")).toEqual({
        alertMessage: "Match result confirmed. This screen will refresh for the next match state.",
        refreshMatchData: true,
        clearSelectedWinner: true,
        lockVoteForCurrentMatch: false
    });
});

// Verifies a stale-match error tells the user the match is gone and refreshes data.
test("MATCH_NOT_ACTIVE error returns inactive-match alert and refreshes match data", () =>
{
    expect(buildErrorFeedback("MATCH_NOT_ACTIVE")).toEqual({
        alertMessage: "That match is no longer active. This screen will refresh now.",
        refreshMatchData: true,
        clearSelectedWinner: true,
        lockVoteForCurrentMatch: false
    });
});

// Verifies a CONFLICT error tells the voter to re-vote without refreshing match data.
test("CONFLICT error returns re-vote notice without refreshing", () =>
{
    expect(buildErrorFeedback("CONFLICT")).toEqual({
        noticeMessage: "Votes conflicted. Please vote again for this match.",
        refreshMatchData: false,
        clearSelectedWinner: true,
        lockVoteForCurrentMatch: false
    });
});

// Verifies a DUPLICATE_VOTE error tells the voter they already voted and locks voting.
test("DUPLICATE_VOTE error returns already-voted notice and locks voting", () =>
{
    expect(buildErrorFeedback("DUPLICATE_VOTE")).toEqual({
        noticeMessage: "You already voted for this match. Waiting for the other player.",
        refreshMatchData: false,
        clearSelectedWinner: true,
        lockVoteForCurrentMatch: true
    });
});

// Verifies a VOTER_NOT_PARTICIPANT error restricts the action to participants only.
test("VOTER_NOT_PARTICIPANT error returns participant-only alert without clearing selection", () =>
{
    expect(buildErrorFeedback("VOTER_NOT_PARTICIPANT", 403)).toEqual({
        alertMessage: "Only the players in this match can vote.",
        refreshMatchData: false,
        clearSelectedWinner: false,
        lockVoteForCurrentMatch: false
    });
});

// Verifies an unknown error message falls back to a generic stay-on-screen notice.
test("unknown error returns generic stay-on-screen notice", () =>
{
    expect(buildErrorFeedback("UNKNOWN")).toEqual({
        alertMessage: "We could not submit that vote. You will stay on this screen so you can try again.",
        refreshMatchData: false,
        clearSelectedWinner: false,
        lockVoteForCurrentMatch: false
    });
});

// Verifies a non-PENDING, non-COMMITTED response defaults to a refresh notice.
test("unrecognised response status returns default refresh notice", () =>
{
    // Cast to a non-PENDING, non-COMMITTED status to exercise the default branch.
    const feedback = getVoteFeedbackFromResponse({
        gameId: "game-id",
        matchId: "match-id",
        // @ts-expect-error - intentional non-standard status to exercise the default branch
        status: "WEIRD_STATUS",
        voteCount: 0,
        committedWinnerPlayerId: undefined
    });

    expect(feedback).toEqual({
        alertMessage: "Vote submitted, but the match state changed. This screen will refresh now.",
        refreshMatchData: true,
        clearSelectedWinner: true,
        lockVoteForCurrentMatch: false
    });
});
