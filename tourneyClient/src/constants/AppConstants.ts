

export const SERVER_ERROR = (tag: string) => 
{
    return `A server error occurred. Please try again later: [${tag}]`;
}

export const SUBMIT_SUCCESS = (tag: string) =>
{
    return `Submission Success: [${tag}]`;
}


export const INVALID_CHARACTERS = (tag: string) =>
{
    return `Invalid Characters in Submission: ${tag}`;
}

export const MAX_SUPPORTED_PLAYERS = 128;