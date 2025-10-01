import { INVALID_CHARACTERS } from "@/constants/StatusMessages";
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


/**
 * Example usage:
 *
 * import { validationService } from './validationService';
 *
 * const { validateInput } = validationService();
 * const result = validateInput(userInput);
 *
 * if (!result.isValid) {
 *   console.warn("Validation issues:", result.issues);
 *   // Handle invalid input (e.g., show error messages)
 * } else {
 *   // Proceed with safe input
 * }
 *
 * This service checks for:
 * - HTML injection
 * - SQL injection
 * - JavaScript injection
 * - Open redirects
 * - Escape character misuse
 */
