// Describes the registration payload the server expects.
//
// All three fields matter: the username is what people sign in with, and the
// address is where the confirmation link goes. The form used to send only two of
// them and silently drop the address.
export type RegisterAccountRequest = {
    userName: string;
    email: string;
    password: string;
};

// Describes the server's answer to a successful registration.
export type RegisterAccountResponse = {
    message: string;
    requiresEmailConfirmation: boolean;
};

// Turns a failed registration into something the person can act on.
//
// The server answers a rejected password or a taken address with the specific
// reasons, and those reasons are the only part of the response worth reading:
// somebody told "try again" will retype the same password that was just refused.
// The reasons ride inside the error message because RequestService throws with
// the response body attached.
export const describeRegistrationFailure = (errorMessage: string): string =>
{
    if (errorMessage.includes("503"))
    {
        return "We could not send the confirmation email, so the account was not created. Try again in a moment.";
    }

    if (errorMessage.includes("429"))
    {
        return "Too many attempts. Wait a minute and try again.";
    }

    const reasons = extractReasons(errorMessage);
    if (reasons.length > 0)
    {
        return reasons.join(" ");
    }

    return "We could not create your account. Try again.";
};

// Pulls the server's list of refusal reasons out of an error message.
const extractReasons = (errorMessage: string): string[] =>
{
    const bodyStart = errorMessage.indexOf("{");
    if (bodyStart === -1)
    {
        return [];
    }

    try
    {
        const body = JSON.parse(errorMessage.slice(bodyStart));

        if (Array.isArray(body?.reasons))
        {
            return body.reasons.filter((reason: unknown): reason is string => typeof reason === "string");
        }

        if (typeof body?.message === "string")
        {
            return [body.message];
        }

        return [];
    }
    catch
    {
        return [];
    }
};
