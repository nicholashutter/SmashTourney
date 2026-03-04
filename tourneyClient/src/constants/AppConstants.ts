export const SERVER_ERROR = (tag: string): string =>
    `We could not complete ${tag}. Please stay on this screen and try again.`;

export const SUBMIT_SUCCESS = (tag: string): string =>
    `${tag} completed successfully.`;

export const INVALID_CHARACTERS = (tag: string): string =>
    `Please remove unsupported characters from ${tag} and try again.`;

export const MAX_SUPPORTED_PLAYERS = 128;