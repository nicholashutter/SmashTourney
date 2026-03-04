import { expect, test } from "vitest";
import { getVoteFeedbackFromError, getVoteFeedbackFromResponse } from "../src/services/matchVoteFeedback";

// Verifies pending vote response keeps player on page and shows waiting notice.
test("Vote feedback returns pending notice behavior", () =>
{
    const feedback = getVoteFeedbackFromResponse({
        gameId: "game-id",
        matchId: "match-id",
        status: "PENDING",
        voteCount: 1,
        committedWinnerPlayerId: undefined
    });

    expect(feedback.noticeMessage).toContain("Waiting for the other player");
    expect(feedback.refreshMatchData).toBe(false);
    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies committed response refreshes state and shows confirmation alert.
test("Vote feedback returns committed refresh behavior", () =>
{
    const feedback = getVoteFeedbackFromResponse({
        gameId: "game-id",
        matchId: "match-id",
        status: "COMMITTED",
        voteCount: 2,
        committedWinnerPlayerId: "winner-id"
    });

    expect(feedback.alertMessage).toContain("Match result confirmed");
    expect(feedback.refreshMatchData).toBe(true);
    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies stale match error triggers refresh and clear-selection behavior.
test("Vote feedback handles MATCH_NOT_ACTIVE as critical refresh", () =>
{
    const feedback = getVoteFeedbackFromError("HTTP 409: {\"status\":\"MATCH_NOT_ACTIVE\"}");

    expect(feedback.alertMessage).toContain("no longer active");
    expect(feedback.refreshMatchData).toBe(true);
    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies conflicting votes keep user in match and request re-vote.
test("Vote feedback handles CONFLICT as re-vote notice", () =>
{
    const feedback = getVoteFeedbackFromError("HTTP 409: {\"status\":\"CONFLICT\"}");

    expect(feedback.noticeMessage).toContain("Please vote again");
    expect(feedback.refreshMatchData).toBe(false);
    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies duplicate votes produce waiting notice and no refresh.
test("Vote feedback handles DUPLICATE_VOTE as wait notice", () =>
{
    const feedback = getVoteFeedbackFromError("HTTP 409: {\"status\":\"DUPLICATE_VOTE\"}");

    expect(feedback.noticeMessage).toContain("already voted");
    expect(feedback.refreshMatchData).toBe(false);
    expect(feedback.clearSelectedWinner).toBe(true);
});

// Verifies non-participant error shows alert and preserves selection state.
test("Vote feedback handles VOTER_NOT_PARTICIPANT with alert", () =>
{
    const feedback = getVoteFeedbackFromError("HTTP 403: {\"status\":\"VOTER_NOT_PARTICIPANT\"}");

    expect(feedback.alertMessage).toContain("Only the players");
    expect(feedback.refreshMatchData).toBe(false);
    expect(feedback.clearSelectedWinner).toBe(false);
});