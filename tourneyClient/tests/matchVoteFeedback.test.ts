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

// Verifies pending response shows waiting notice text.
test("pending response includes waiting notice", () =>
{
    const feedback = buildResponseFeedback("PENDING");

    expect(feedback.noticeMessage).toContain("Waiting for the other player");
});

// Verifies pending response does not trigger match refresh.
test("pending response does not refresh match data", () =>
{
    const feedback = buildResponseFeedback("PENDING");

    expect(feedback.refreshMatchData).toBe(false);
});

// Verifies pending response clears selected winner.
test("pending response clears selected winner", () =>
{
    const feedback = buildResponseFeedback("PENDING");

    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies pending response locks voting for the active match.
test("pending response locks voting for active match", () =>
{
    const feedback = buildResponseFeedback("PENDING");

    expect(feedback.lockVoteForCurrentMatch).toBe(true);
});

// Verifies committed response includes confirmation alert.
test("committed response includes confirmation alert", () =>
{
    const feedback = buildResponseFeedback("COMMITTED");

    expect(feedback.alertMessage).toContain("Match result confirmed");
});

// Verifies committed response refreshes match data.
test("committed response refreshes match data", () =>
{
    const feedback = buildResponseFeedback("COMMITTED");

    expect(feedback.refreshMatchData).toBe(true);
});

// Verifies committed response clears selected winner.
test("committed response clears selected winner", () =>
{
    const feedback = buildResponseFeedback("COMMITTED");

    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies committed response does not keep voting locked.
test("committed response does not lock voting for active match", () =>
{
    const feedback = buildResponseFeedback("COMMITTED");

    expect(feedback.lockVoteForCurrentMatch).toBe(false);
});

// Verifies stale-match error includes inactive-match alert.
test("MATCH_NOT_ACTIVE error returns inactive-match alert", () =>
{
    const feedback = buildErrorFeedback("MATCH_NOT_ACTIVE");

    expect(feedback.alertMessage).toContain("no longer active");
});

// Verifies stale-match error requests match refresh.
test("MATCH_NOT_ACTIVE error refreshes match data", () =>
{
    const feedback = buildErrorFeedback("MATCH_NOT_ACTIVE");

    expect(feedback.refreshMatchData).toBe(true);
});

// Verifies stale-match error clears selected winner.
test("MATCH_NOT_ACTIVE error clears selected winner", () =>
{
    const feedback = buildErrorFeedback("MATCH_NOT_ACTIVE");

    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies conflict error returns a re-vote notice.
test("CONFLICT error returns re-vote notice", () =>
{
    const feedback = buildErrorFeedback("CONFLICT");

    expect(feedback.noticeMessage).toContain("Please vote again");
});

// Verifies conflict error does not refresh match data.
test("CONFLICT error does not refresh match data", () =>
{
    const feedback = buildErrorFeedback("CONFLICT");

    expect(feedback.refreshMatchData).toBe(false);
});

// Verifies conflict error clears selected winner.
test("CONFLICT error clears selected winner", () =>
{
    const feedback = buildErrorFeedback("CONFLICT");

    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies conflict error does not lock voting and allows re-vote.
test("CONFLICT error does not lock voting for active match", () =>
{
    const feedback = buildErrorFeedback("CONFLICT");

    expect(feedback.lockVoteForCurrentMatch).toBe(false);
});

// Verifies duplicate-vote error returns already-voted notice.
test("DUPLICATE_VOTE error returns already-voted notice", () =>
{
    const feedback = buildErrorFeedback("DUPLICATE_VOTE");

    expect(feedback.noticeMessage).toContain("already voted");
});

// Verifies duplicate-vote error does not refresh match data.
test("DUPLICATE_VOTE error does not refresh match data", () =>
{
    const feedback = buildErrorFeedback("DUPLICATE_VOTE");

    expect(feedback.refreshMatchData).toBe(false);
});

// Verifies duplicate-vote error clears selected winner.
test("DUPLICATE_VOTE error clears selected winner", () =>
{
    const feedback = buildErrorFeedback("DUPLICATE_VOTE");

    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies duplicate-vote error keeps voting locked for waiting state.
test("DUPLICATE_VOTE error locks voting for active match", () =>
{
    const feedback = buildErrorFeedback("DUPLICATE_VOTE");

    expect(feedback.lockVoteForCurrentMatch).toBe(true);
});

// Verifies non-participant error includes participant-only alert.
test("VOTER_NOT_PARTICIPANT error returns participant-only alert", () =>
{
    const feedback = buildErrorFeedback("VOTER_NOT_PARTICIPANT", 403);

    expect(feedback.alertMessage).toContain("Only the players");
});

// Verifies non-participant error does not refresh match data.
test("VOTER_NOT_PARTICIPANT error does not refresh match data", () =>
{
    const feedback = buildErrorFeedback("VOTER_NOT_PARTICIPANT", 403);

    expect(feedback.refreshMatchData).toBe(false);
});

// Verifies non-participant error keeps selected winner unchanged.
test("VOTER_NOT_PARTICIPANT error keeps selected winner", () =>
{
    const feedback = buildErrorFeedback("VOTER_NOT_PARTICIPANT", 403);

    expect(feedback.clearSelectedWinner).toBe(false);
});