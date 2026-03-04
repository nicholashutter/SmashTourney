import { SubmitMatchVoteResponse } from "@/models/entities/Bracket";

// Defines the UI action to take after processing a vote response or vote error.
export type VoteFeedback = {
    alertMessage?: string;
    noticeMessage?: string;
    refreshMatchData: boolean;
    clearSelectedWinner: boolean;
};

// Maps successful vote responses to frontend UI actions.
export const getVoteFeedbackFromResponse = (
    voteResponse: SubmitMatchVoteResponse
): VoteFeedback =>
{
    if (voteResponse.status === "PENDING")
    {
        return {
            noticeMessage: "Your vote is locked in. Waiting for the other player to vote.",
            refreshMatchData: false,
            clearSelectedWinner: true
        };
    }

    if (voteResponse.status === "COMMITTED")
    {
        return {
            alertMessage: "Match result confirmed. This screen will refresh for the next match state.",
            refreshMatchData: true,
            clearSelectedWinner: true
        };
    }

    return {
        alertMessage: "Vote submitted, but the match state changed. This screen will refresh now.",
        refreshMatchData: true,
        clearSelectedWinner: true
    };
};

// Maps failed vote submissions to frontend UI actions.
export const getVoteFeedbackFromError = (errorMessage: string): VoteFeedback =>
{
    if (errorMessage.includes("MATCH_NOT_ACTIVE"))
    {
        return {
            alertMessage: "That match is no longer active. This screen will refresh now.",
            refreshMatchData: true,
            clearSelectedWinner: true
        };
    }

    if (errorMessage.includes("CONFLICT"))
    {
        return {
            noticeMessage: "Votes conflicted. Please vote again for this match.",
            refreshMatchData: false,
            clearSelectedWinner: true
        };
    }

    if (errorMessage.includes("DUPLICATE_VOTE"))
    {
        return {
            noticeMessage: "You already voted for this match. Waiting for the other player.",
            refreshMatchData: false,
            clearSelectedWinner: true
        };
    }

    if (errorMessage.includes("VOTER_NOT_PARTICIPANT"))
    {
        return {
            alertMessage: "Only the players in this match can vote.",
            refreshMatchData: false,
            clearSelectedWinner: false
        };
    }

    return {
        alertMessage: "We could not submit that vote. You will stay on this screen so you can try again.",
        refreshMatchData: false,
        clearSelectedWinner: false
    };
};