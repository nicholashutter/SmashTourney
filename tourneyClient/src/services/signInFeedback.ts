// Turns a failed sign-in into words that tell somebody what to do next.
//
// Sign-in can now fail for reasons that are not "wrong password", and they need
// different actions: an unconfirmed address means go and read your email, a
// lockout means wait, and a rate limit means wait rather than keep hammering. A
// single "check your details" message for all of these sends somebody to retype
// a password that was already correct.
export const describeSignInFailure = (errorMessage: string): string =>
{
    if (errorMessage.includes("403"))
    {
        return "Confirm your email address before signing in. Check your inbox for the link.";
    }

    if (errorMessage.includes("423"))
    {
        return "Too many failed attempts. Wait a few minutes and try again.";
    }

    if (errorMessage.includes("429"))
    {
        return "Too many attempts. Wait a minute and try again.";
    }

    if (errorMessage.includes("401"))
    {
        return "That username or password is not right.";
    }

    return "Sign in failed. Check your connection and try again.";
};
