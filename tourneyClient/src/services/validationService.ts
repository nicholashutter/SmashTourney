import { INVALID_CHARACTERS, MAX_SUPPORTED_PLAYERS } from "@/constants/AppConstants";
type ValidationResult = {
    isValid: boolean;
    issues: string[];
};

export const validateInput = (input: string): ValidationResult =>
{
    const issues: string[] = [];




    const htmlPattern = /<[^>]*>/g;
    if (htmlPattern.test(input))
    {
        issues.push(INVALID_CHARACTERS("HTML"));
    }


    const sqlPattern = /\b(SELECT|INSERT|DELETE|UPDATE|DROP|UNION|--|;)\b/i;
    if (sqlPattern.test(input))
    {
        issues.push(INVALID_CHARACTERS("SQL"));
    }


    const jsPattern = /\b(script|onerror|onload|eval|alert|document\.cookie)\b/i;
    if (jsPattern.test(input))
    {
        issues.push(INVALID_CHARACTERS("JS"));
    }


    const redirectPattern = /(https?:\/\/|\/\/)[^\s]+/i;
    if (redirectPattern.test(input))
    {
        issues.push(INVALID_CHARACTERS("URL"));
    }


    const escapePattern = /[\\'"]/;
    if (escapePattern.test(input))
    {
        issues.push(INVALID_CHARACTERS("ESC"));
    }


    return {
        isValid: issues.length === 0,
        issues,
    };
};

export const validateGameIdResponse = (gameId: string) =>
{
    if (typeof gameId !== "string")
    {
        return false;
    }

    return validateInput(gameId).isValid;
};

export const validateTotalPlayers = (userInput: number) =>
{
    if (typeof userInput !== "number")
    {
        return false;
    }

    if (userInput <= 0 || userInput >= MAX_SUPPORTED_PLAYERS)
    {
        return false;
    }

    return validateInput(userInput.toString()).isValid;
};
